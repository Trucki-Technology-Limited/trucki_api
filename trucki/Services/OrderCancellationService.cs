using AutoMapper;
using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services
{
    public interface IOrderCancellationService
    {
        Task<ApiResponseModel<CancellationPreviewResponseModel>> GetCancellationPreviewAsync(string orderId, string cargoOwnerId);
        Task<ApiResponseModel<OrderCancellationResponseModel>> CancelOrderAsync(CancelOrderRequestDto request);
        Task<ApiResponseModel<bool>> ProcessCancellationRefundAsync(ProcessCancellationRefundDto request);

    }

    public class OrderCancellationService : IOrderCancellationService
    {
        private readonly TruckiDBContext _dbContext;
        private readonly ILogger<OrderCancellationService> _logger;
        private readonly IWalletService _walletService;
        private readonly IStripeService _stripeService;
        private readonly NotificationEventService _notificationEventService;

        public OrderCancellationService(
            TruckiDBContext dbContext,
            ILogger<OrderCancellationService> logger,
            IWalletService walletService,
            IStripeService stripeService,
            NotificationEventService notificationEventService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _walletService = walletService;
            _stripeService = stripeService;
            _notificationEventService = notificationEventService;
        }

        public async Task<ApiResponseModel<CancellationPreviewResponseModel>> GetCancellationPreviewAsync(string orderId, string cargoOwnerId)
        {
            try
            {
                var order = await _dbContext.Set<CargoOrders>()
                    .Include(o => o.AcceptedBid)
                    .Include(o => o.CargoOwner)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.CargoOwnerId == cargoOwnerId);

                if (order == null)
                {
                    return ApiResponseModel<CancellationPreviewResponseModel>.Fail("Order not found", 404);
                }

                var canCancel = CanOrderBeCancelled(order.Status);
                if (!canCancel.CanCancel)
                {
                    return ApiResponseModel<CancellationPreviewResponseModel>.Fail(canCancel.Reason, 400);
                }

                var penaltyInfo = CalculatePenalty(order);

                var preview = new CancellationPreviewResponseModel
                {
                    OrderId = orderId,
                    CanCancel = true,
                    CancellationReason = canCancel.Reason,
                    PenaltyPercentage = penaltyInfo.PenaltyPercentage,
                    PenaltyAmount = penaltyInfo.PenaltyAmount,
                    RefundAmount = order.IsPaid ? order.TotalAmount - penaltyInfo.PenaltyAmount : 0,
                    OriginalAmount = order.TotalAmount,
                    PenaltyJustification = penaltyInfo.Justification,
                    RequiresConfirmation = penaltyInfo.PenaltyAmount > 0,
                    Warnings = new List<string>()
                };

                // Add specific warnings based on user type
                if (order.CargoOwner.OwnerType == CargoOwnerType.Broker && !order.IsPaid && penaltyInfo.PenaltyAmount > 0)
                {
                    preview.Warnings.Add("As a broker, you must pay the cancellation fee before canceling this order.");
                }
                else if (order.CargoOwner.OwnerType == CargoOwnerType.Shipper && penaltyInfo.PenaltyAmount > 0)
                {
                    preview.Warnings.Add("Cancellation fee will be deducted from your refund amount.");
                }

                return ApiResponseModel<CancellationPreviewResponseModel>.Success(
                    "Cancellation preview generated successfully",
                    preview,
                    200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating cancellation preview for order {OrderId}", orderId);
                return ApiResponseModel<CancellationPreviewResponseModel>.Fail($"Error: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponseModel<OrderCancellationResponseModel>> CancelOrderAsync(CancelOrderRequestDto request)
        {
            try
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                var order = await _dbContext.Set<CargoOrders>()
                    .Include(o => o.AcceptedBid)
                        .ThenInclude(b => b.Truck)
                            .ThenInclude(t => t.Driver)
                    .Include(o => o.CargoOwner)
                    .Include(o => o.Bids)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.CargoOwnerId == request.CargoOwnerId);

                if (order == null)
                {
                    return ApiResponseModel<OrderCancellationResponseModel>.Fail("Order not found", 404);
                }

                var canCancel = CanOrderBeCancelled(order.Status);
                if (!canCancel.CanCancel)
                {
                    return ApiResponseModel<OrderCancellationResponseModel>.Fail(canCancel.Reason, 400);
                }

                var penaltyInfo = CalculatePenalty(order);

                // Handle broker vs shipper cancellation logic
                if (order.CargoOwner.OwnerType == CargoOwnerType.Broker)
                {
                    return await HandleBrokerCancellation(order, request, penaltyInfo, transaction);
                }
                else
                {
                    return await HandleShipperCancellation(order, request, penaltyInfo, transaction);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", request.OrderId);
                return ApiResponseModel<OrderCancellationResponseModel>.Fail($"Error: {ex.Message}", 500);
            }
        }

        private async Task<ApiResponseModel<OrderCancellationResponseModel>> HandleBrokerCancellation(
            CargoOrders order,
            CancelOrderRequestDto request,
            (decimal PenaltyPercentage, decimal PenaltyAmount, string Justification) penaltyInfo,
            Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction)
        {
            // For brokers: if there's a penalty and order is not paid, they must pay the cancellation fee first
            if (penaltyInfo.PenaltyAmount > 0 && !order.IsPaid)
            {
                if (string.IsNullOrEmpty(request.CancellationFeePaymentIntentId))
                {
                    // Create payment intent for cancellation fee
                    var paymentIntent = await _stripeService.CreatePaymentIntent(
                        $"cancellation-{order.Id}",
                        penaltyInfo.PenaltyAmount,
                        "usd");

                    await transaction.RollbackAsync();
                    return ApiResponseModel<OrderCancellationResponseModel>.Fail(
                        $"Cancellation fee payment required. Penalty: ${penaltyInfo.PenaltyAmount:F2}",

                     402);
                }
                else
                {
                    // Verify the cancellation fee payment
                    var paymentVerified = await _stripeService.VerifyPaymentStatus(request.CancellationFeePaymentIntentId);
                    if (!paymentVerified)
                    {
                        await transaction.RollbackAsync();
                        return ApiResponseModel<OrderCancellationResponseModel>.Fail(
                            "Cancellation fee payment verification failed", 400);
                    }
                }
            }

            // Process the cancellation for broker
            var cancellation = await CreateCancellationRecord(order, request, penaltyInfo);

            // For brokers, if order was paid, process refund. If not paid, just void the invoice
            ApiResponseModel<bool> refundResult;
            if (order.IsPaid)
            {
                refundResult = await ProcessRefundAsync(order, cancellation);
            }
            else
            {
                refundResult = await ProcessInvoiceVoid(order, cancellation);
            }

            if (!refundResult.IsSuccessful)
            {
                cancellation.Status = CancellationStatus.RefundPending;
                cancellation.AdminNotes = $"Marked for admin review. Error: {refundResult.Message}";
                _logger.LogWarning("Broker cancellation requires manual review for order {OrderId}. Error: {Error}",
                    order.Id, refundResult.Message);
            }
            else
            {
                cancellation.Status = CancellationStatus.RefundProcessed;
                cancellation.RefundProcessedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            // Send notifications
            await SendCancellationNotificationsAsync(order, cancellation, order.Status);

            return ApiResponseModel<OrderCancellationResponseModel>.Success(
                "Order cancelled successfully",
                BuildCancellationResponse(order, cancellation, penaltyInfo, refundResult),
                200);
        }

        private async Task<ApiResponseModel<OrderCancellationResponseModel>> HandleShipperCancellation(
            CargoOrders order,
            CancelOrderRequestDto request,
            (decimal PenaltyPercentage, decimal PenaltyAmount, string Justification) penaltyInfo,
            Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction)
        {
            // For shippers: if there's a penalty and user hasn't acknowledged it
            if (penaltyInfo.PenaltyAmount > 0 && !request.AcknowledgePenalty)
            {
                await transaction.RollbackAsync();
                return ApiResponseModel<OrderCancellationResponseModel>.Fail(
                    $"Cancellation requires penalty acknowledgment. Penalty: ${penaltyInfo.PenaltyAmount:F2} ({penaltyInfo.Justification})",
                    400);
            }

            // Process the cancellation for shipper
            var cancellation = await CreateCancellationRecord(order, request, penaltyInfo);

            // For shippers, if order was paid, deduct penalty and refund the rest
            ApiResponseModel<bool> refundResult;
            if (order.IsPaid && penaltyInfo.PenaltyAmount > 0)
            {
                // First deduct the penalty fee from their wallet/account
                var penaltyResult = await ProcessPenaltyDeduction(order, penaltyInfo.PenaltyAmount);
                if (!penaltyResult.IsSuccessful)
                {
                    await transaction.RollbackAsync();
                    return ApiResponseModel<OrderCancellationResponseModel>.Fail(
                        $"Error processing penalty deduction: {penaltyResult.Message}", 500);
                }

                // Then process the refund for the remaining amount
                refundResult = await ProcessRefundAsync(order, cancellation);
            }
            else if (order.IsPaid)
            {
                // No penalty, full refund
                refundResult = await ProcessRefundAsync(order, cancellation);
            }
            else
            {
                // Order not paid, just void the invoice
                refundResult = await ProcessInvoiceVoid(order, cancellation);
            }

            if (!refundResult.IsSuccessful)
            {
                cancellation.Status = CancellationStatus.RefundPending;
                cancellation.AdminNotes = $"Marked for admin review. Error: {refundResult.Message}";
                _logger.LogWarning("Shipper cancellation requires manual review for order {OrderId}. Error: {Error}",
                    order.Id, refundResult.Message);
            }
            else
            {
                cancellation.Status = CancellationStatus.RefundProcessed;
                cancellation.RefundProcessedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            // Send notifications
            await SendCancellationNotificationsAsync(order, cancellation, order.Status);

            return ApiResponseModel<OrderCancellationResponseModel>.Success(
                "Order cancelled successfully",
                BuildCancellationResponse(order, cancellation, penaltyInfo, refundResult),
                200);
        }

        private async Task<ApiResponseModel<bool>> ProcessPenaltyDeduction(CargoOrders order, decimal penaltyAmount)
        {
            try
            {
                // Create a transaction record for the penalty deduction
                var penaltyResult = await _walletService.DeductFundsFromWallet(
                    order.CargoOwnerId,
                    penaltyAmount,
                    $"Cancellation penalty for order #{order.Id}",
                    WalletTransactionType.CancellationFee,
                    order.Id);

                if (penaltyResult.IsSuccessful)
                {
                    _logger.LogInformation("Penalty of ${PenaltyAmount} deducted for order {OrderId}",
                        penaltyAmount, order.Id);
                }

                return penaltyResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing penalty deduction for order {OrderId}", order.Id);
                return ApiResponseModel<bool>.Fail($"Penalty deduction failed: {ex.Message}", 500);
            }
        }

        private async Task<OrderCancellation> CreateCancellationRecord(
            CargoOrders order,
            CancelOrderRequestDto request,
            (decimal PenaltyPercentage, decimal PenaltyAmount, string Justification) penaltyInfo)
        {
            var originalStatus = order.Status;
            order.Status = CargoOrderStatus.Cancelled;
            // Note: CargoOrders doesn't have CancelledAt/CancellationReason properties
            // These are tracked in the OrderCancellation entity instead

            var cancellation = new OrderCancellation
            {
                OrderId = order.Id,
                CargoOwnerId = order.CargoOwnerId,
                CancelledAt = DateTime.UtcNow,
                CancellationReason = request.CancellationReason,
                OrderStatusAtCancellation = originalStatus,
                OriginalAmount = order.TotalAmount,
                PenaltyPercentage = penaltyInfo.PenaltyPercentage,
                PenaltyAmount = penaltyInfo.PenaltyAmount,
                RefundAmount = order.IsPaid ? order.TotalAmount - penaltyInfo.PenaltyAmount : 0,
                OriginalPaymentMethod = order.IsPaid ? order.PaymentMethod : PaymentMethodType.Invoice,
                RefundMethod = DetermineRefundMethod(order),
                Status = CancellationStatus.Approved,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Set<OrderCancellation>().Add(cancellation);
            return cancellation;
        }

        private OrderCancellationResponseModel BuildCancellationResponse(
            CargoOrders order,
            OrderCancellation cancellation,
            (decimal PenaltyPercentage, decimal PenaltyAmount, string Justification) penaltyInfo,
            ApiResponseModel<bool> refundResult)
        {
            var response = new OrderCancellationResponseModel
            {
                OrderId = order.Id,
                Status = "Cancelled",
                CancellationDetails = new CancellationDetails
                {
                    CancelledAt = cancellation.CancelledAt,
                    CancellationReason = cancellation.CancellationReason,
                    PenaltyPercentage = cancellation.PenaltyPercentage,
                    PenaltyAmount = cancellation.PenaltyAmount,
                    RequiresApproval = false,
                    PenaltyJustification = penaltyInfo.Justification
                },
                RefundDetails = new RefundDetails
                {
                    OriginalAmount = cancellation.OriginalAmount,
                    PenaltyAmount = cancellation.PenaltyAmount,
                    RefundAmount = cancellation.RefundAmount,
                    OriginalPaymentMethod = cancellation.OriginalPaymentMethod ?? PaymentMethodType.Invoice,
                    RefundMethod = cancellation.RefundMethod,
                    RefundStatus = refundResult.IsSuccessful ? "Processed" : "Pending",
                    RefundProcessedAt = cancellation.RefundProcessedAt
                },
                Message = order.CargoOwner.OwnerType == CargoOwnerType.Broker
                    ? (refundResult.IsSuccessful
                        ? $"Order cancelled successfully. {(cancellation.RefundAmount > 0 ? $"Refund of ${cancellation.RefundAmount:F2} has been processed." : "Invoice has been voided.")}"
                        : $"Order cancelled successfully. {(cancellation.RefundAmount > 0 ? $"Refund of ${cancellation.RefundAmount:F2} is being processed." : "Invoice processing pending.")}")
                    : (refundResult.IsSuccessful
                        ? $"Order cancelled successfully. {(cancellation.PenaltyAmount > 0 ? $"Penalty of ${cancellation.PenaltyAmount:F2} has been deducted. " : "")}Refund of ${cancellation.RefundAmount:F2} has been processed."
                        : $"Order cancelled successfully. {(cancellation.PenaltyAmount > 0 ? $"Penalty of ${cancellation.PenaltyAmount:F2} has been deducted. " : "")}Refund of ${cancellation.RefundAmount:F2} is being processed.")
            };

            return response;
        }



        private (bool CanCancel, string Reason) CanOrderBeCancelled(CargoOrderStatus status)
        {
            return status switch
            {
                CargoOrderStatus.Draft => (true, "Order can be cancelled"),
                CargoOrderStatus.OpenForBidding => (true, "Order can be cancelled"),
                CargoOrderStatus.BiddingInProgress => (true, "Order can be cancelled"),
                CargoOrderStatus.DriverSelected => (true, "Order can be cancelled"),
                CargoOrderStatus.DriverAcknowledged => (true, "Order can be cancelled"),
                CargoOrderStatus.InTransit => (false, "Cannot cancel order in transit"),
                CargoOrderStatus.Delivered => (false, "Cannot cancel delivered order"),
                CargoOrderStatus.Completed => (false, "Cannot cancel completed order"),
                CargoOrderStatus.PaymentOverdue => (false, "Cannot cancel order with overdue payment"),
                CargoOrderStatus.Cancelled => (false, "Cannot cancel order with overdue payment"),
                _ => (false, "Invalid order status")
            };
        }

        private (decimal PenaltyPercentage, decimal PenaltyAmount, string Justification) CalculatePenalty(CargoOrders order)
        {
            // No penalty if no bids received
            if (order.Status == CargoOrderStatus.Draft ||
                order.Status == CargoOrderStatus.OpenForBidding ||
                (order.Status == CargoOrderStatus.BiddingInProgress && order.AcceptedBidId == null))
            {
                return (0m, 0m, "No penalty as no driver has been selected");
            }

            // 2% penalty if bidding has started and a driver has acknowledged the order
            if (order.Status == CargoOrderStatus.DriverAcknowledged && order.AcceptedBidId != null)
            {
                var penaltyPercentage = 2m;
                var penaltyAmount = order.TotalAmount * (penaltyPercentage / 100);
                return (penaltyPercentage, penaltyAmount,
                    "2% penalty applies as a driver has been selected and acknowledged the order");
            }

            // 1% penalty if driver is selected but not yet acknowledged
            if (order.Status == CargoOrderStatus.DriverSelected && order.AcceptedBidId != null)
            {
                var penaltyPercentage = 1m;
                var penaltyAmount = order.TotalAmount * (penaltyPercentage / 100);
                return (penaltyPercentage, penaltyAmount,
                    "1% penalty applies as a driver has been selected");
            }

            return (0m, 0m, "No penalty applicable");
        }

        private trucki.Entities.RefundMethod DetermineRefundMethod(CargoOrders order)
        {
            if (!order.IsPaid)
            {
                return trucki.Entities.RefundMethod.InvoiceVoid;
            }

            return order.PaymentMethod switch
            {
                PaymentMethodType.Stripe => trucki.Entities.RefundMethod.Wallet,
                PaymentMethodType.Wallet => trucki.Entities.RefundMethod.Wallet,
                PaymentMethodType.Mixed => trucki.Entities.RefundMethod.Wallet,
                PaymentMethodType.Invoice => trucki.Entities.RefundMethod.InvoiceVoid,
                _ => trucki.Entities.RefundMethod.Wallet
            };
        }

        private async Task<ApiResponseModel<bool>> ProcessRefundAsync(CargoOrders order, OrderCancellation cancellation)
        {
            try
            {
                var description = cancellation.PenaltyAmount > 0
                    ? $"Refund for cancelled order #{order.Id}. Original: ${cancellation.OriginalAmount:F2}, Penalty: ${cancellation.PenaltyAmount:F2}"
                    : $"Full refund for cancelled order #{order.Id}";

                var walletResult = await _walletService.AddFundsToWallet(
                    order.CargoOwnerId,
                    cancellation.RefundAmount,
                    description,
                    WalletTransactionType.Refund,
                    order.Id);

                if (walletResult.IsSuccessful)
                {
                    await _notificationEventService.NotifyRefundProcessed(
                        order.CargoOwnerId,
                        order.Id,
                        cancellation.RefundAmount,
                        "Wallet");
                }

                return walletResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing wallet refund for order {OrderId}", order.Id);
                return ApiResponseModel<bool>.Fail($"Wallet refund failed: {ex.Message}", 500);
            }
        }

        private async Task<ApiResponseModel<bool>> ProcessInvoiceVoid(CargoOrders order, OrderCancellation cancellation)
        {
            try
            {
                var invoice = await _dbContext.Set<Invoice>()
                    .FirstOrDefaultAsync(i => i.OrderId == order.Id);

                if (invoice != null)
                {
                    invoice.Status = InvoiceStatus.Cancelled;
                    invoice.PaymentNotes = $"Invoice voided due to order cancellation. Penalty: ${cancellation.PenaltyAmount:F2}";
                    await _dbContext.SaveChangesAsync();
                }

                return ApiResponseModel<bool>.Success("Invoice voided successfully", true, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voiding invoice for order {OrderId}", order.Id);
                return ApiResponseModel<bool>.Fail($"Invoice void failed: {ex.Message}", 500);
            }
        }

        private async Task SendCancellationNotificationsAsync(CargoOrders order, OrderCancellation cancellation, CargoOrderStatus originalStatus)
        {
            // Implementation for sending notifications
            // This would include notifying drivers, cargo owners, etc.
            await Task.CompletedTask; // Placeholder
        }



        // Additional methods for processing cancellation refunds, etc. (keeping existing implementation)
        public async Task<ApiResponseModel<bool>> ProcessCancellationRefundAsync(ProcessCancellationRefundDto request)
        {
            try
            {
                var cancellation = await _dbContext.Set<OrderCancellation>()
                    .Include(c => c.Order)
                    .FirstOrDefaultAsync(c => c.OrderId == request.OrderId);

                if (cancellation == null)
                {
                    return ApiResponseModel<bool>.Fail("Cancellation record not found", 404);
                }

                if (cancellation.Status == CancellationStatus.RefundProcessed)
                {
                    return ApiResponseModel<bool>.Fail("Refund already processed", 400);
                }

                var refundResult = await ProcessRefundAsync(cancellation.Order, cancellation);

                if (refundResult.IsSuccessful)
                {
                    cancellation.Status = CancellationStatus.RefundProcessed;
                    cancellation.RefundProcessedAt = DateTime.UtcNow;
                    cancellation.ProcessedBy = request.AdminId;
                    cancellation.AdminNotes = request.AdminNotes;

                    await _dbContext.SaveChangesAsync();
                }

                return refundResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for order {OrderId}", request.OrderId);
                return ApiResponseModel<bool>.Fail($"Error: {ex.Message}", 500);
            }
        }
    }
}