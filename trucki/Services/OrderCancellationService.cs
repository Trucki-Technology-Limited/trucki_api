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
        Task<ApiResponseModel<IEnumerable<OrderCancellation>>> GetCancellationHistoryAsync(string cargoOwnerId);
    }

    public class OrderCancellationService : IOrderCancellationService
    {
        private readonly TruckiDBContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IWalletService _walletService;
        private readonly NotificationEventService _notificationEventService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<OrderCancellationService> _logger;

        public OrderCancellationService(
            TruckiDBContext dbContext,
            IMapper mapper,
            IWalletService walletService,
            NotificationEventService notificationEventService,
            INotificationService notificationService,
            ILogger<OrderCancellationService> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _walletService = walletService;
            _notificationEventService = notificationEventService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<ApiResponseModel<CancellationPreviewResponseModel>> GetCancellationPreviewAsync(string orderId, string cargoOwnerId)
        {
            try
            {
                var order = await _dbContext.Set<CargoOrders>()
                    .Include(o => o.AcceptedBid)
                        .ThenInclude(b => b.Truck)
                            .ThenInclude(t => t.Driver)
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
                var refundAmount = order.TotalAmount - penaltyInfo.PenaltyAmount;

                var preview = new CancellationPreviewResponseModel
                {
                    OrderId = orderId,
                    CanCancel = true,
                    PenaltyPercentage = penaltyInfo.PenaltyPercentage,
                    PenaltyAmount = penaltyInfo.PenaltyAmount,
                    RefundAmount = refundAmount,
                    OriginalAmount = order.TotalAmount,
                    PenaltyJustification = penaltyInfo.Justification,
                    RequiresConfirmation = penaltyInfo.PenaltyAmount > 0,
                    Warnings = GenerateWarnings(order, penaltyInfo)
                };

                return ApiResponseModel<CancellationPreviewResponseModel>.Success(
                    "Cancellation preview generated",
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

                // If there's a penalty and user hasn't acknowledged it
                if (penaltyInfo.PenaltyAmount > 0 && !request.AcknowledgePenalty)
                {
                    return ApiResponseModel<OrderCancellationResponseModel>.Fail(
                        $"Cancellation requires penalty acknowledgment. Penalty: ${penaltyInfo.PenaltyAmount:F2} ({penaltyInfo.PenaltyPercentage}%)",
                        400);
                }

                // Create cancellation record
                var cancellation = new OrderCancellation
                {
                    OrderId = order.Id,
                    CargoOwnerId = order.CargoOwnerId,
                    CancellationReason = request.CancellationReason,
                    OrderStatusAtCancellation = order.Status,
                    PenaltyPercentage = penaltyInfo.PenaltyPercentage,
                    PenaltyAmount = penaltyInfo.PenaltyAmount,
                    OriginalAmount = order.TotalAmount,
                    RefundAmount = order.TotalAmount - penaltyInfo.PenaltyAmount,
                    OriginalPaymentMethod = order.PaymentMethod,
                    RefundMethod = DetermineRefundMethod(order),
                    Status = CancellationStatus.Approved,
                    CancelledAt = DateTime.UtcNow,
                    DriverId = order.AcceptedBid?.Truck?.Driver?.Id
                };

                _dbContext.Set<OrderCancellation>().Add(cancellation);

                // Update order status
                var originalStatus = order.Status;
                order.Status = CargoOrderStatus.Cancelled;

                // Mark bids as expired
                foreach (var bid in order.Bids)
                {
                    if (bid.Status == BidStatus.Pending || bid.Status == BidStatus.AdminApproved)
                    {
                        bid.Status = BidStatus.Expired;
                    }
                }

                // Process refund immediately for certain cases
                var refundResult = await ProcessRefundAsync(order, cancellation);
                if (refundResult.IsSuccessful)
                {
                    cancellation.Status = CancellationStatus.RefundProcessed;
                    cancellation.RefundProcessedAt = DateTime.UtcNow;
                }
                else
                {
                    // If automatic refund fails, mark for admin review
                    cancellation.Status = CancellationStatus.RefundPending;
                    _logger.LogWarning("Automatic refund failed for order {OrderId}. Marked for admin review. Error: {Error}", 
                        order.Id, refundResult.Message);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // Send notifications
                await SendCancellationNotificationsAsync(order, cancellation, originalStatus);

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
                    Message = refundResult.IsSuccessful 
                        ? $"Order cancelled successfully. Refund of ${cancellation.RefundAmount:F2} has been processed."
                        : $"Order cancelled successfully. Refund of ${cancellation.RefundAmount:F2} is being processed."
                };

                return ApiResponseModel<OrderCancellationResponseModel>.Success(
                    "Order cancelled successfully",
                    response,
                    200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", request.OrderId);
                return ApiResponseModel<OrderCancellationResponseModel>.Fail($"Error: {ex.Message}", 500);
            }
        }

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

        public async Task<ApiResponseModel<IEnumerable<OrderCancellation>>> GetCancellationHistoryAsync(string cargoOwnerId)
        {
            try
            {
                var cancellations = await _dbContext.Set<OrderCancellation>()
                    .Include(c => c.Order)
                    .Where(c => c.CargoOwnerId == cargoOwnerId)
                    .OrderByDescending(c => c.CancelledAt)
                    .ToListAsync();

                return ApiResponseModel<IEnumerable<OrderCancellation>>.Success(
                    "Cancellation history retrieved",
                    cancellations,
                    200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cancellation history for cargo owner {CargoOwnerId}", cargoOwnerId);
                return ApiResponseModel<IEnumerable<OrderCancellation>>.Fail($"Error: {ex.Message}", 500);
            }
        }

        private (bool CanCancel, string Reason) CanOrderBeCancelled(CargoOrderStatus status)
        {
            return status switch
            {
                CargoOrderStatus.Draft => (true, ""),
                CargoOrderStatus.OpenForBidding => (true, ""),
                CargoOrderStatus.BiddingInProgress => (true, ""),
                CargoOrderStatus.DriverSelected => (true, ""),
                CargoOrderStatus.DriverAcknowledged => (true, ""),
                CargoOrderStatus.ReadyForPickup => (false, "Cannot cancel order that is ready for pickup"),
                CargoOrderStatus.InTransit => (false, "Cannot cancel order that is in transit"),
                CargoOrderStatus.Delivered => (false, "Cannot cancel delivered order"),
                CargoOrderStatus.Completed => (false, "Cannot cancel completed order"),
                CargoOrderStatus.PaymentPending => (false, "Cannot cancel order with pending payment"),
                CargoOrderStatus.PaymentComplete => (false, "Cannot cancel order with completed payment"),
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

            // All refunds go to wallet regardless of original payment method
            return order.PaymentMethod switch
            {
                PaymentMethodType.Wallet => trucki.Entities.RefundMethod.Wallet,
                PaymentMethodType.Stripe => trucki.Entities.RefundMethod.Wallet,
                PaymentMethodType.Mixed => trucki.Entities.RefundMethod.Wallet,
                PaymentMethodType.Invoice => trucki.Entities.RefundMethod.InvoiceVoid,
                _ => trucki.Entities.RefundMethod.Wallet
            };
        }

        private async Task<ApiResponseModel<bool>> ProcessRefundAsync(CargoOrders order, OrderCancellation cancellation)
        {
            try
            {
                if (cancellation.RefundAmount <= 0)
                {
                    return ApiResponseModel<bool>.Success("No refund required", true, 200);
                }

                switch (cancellation.RefundMethod)
                {
                    case trucki.Entities.RefundMethod.Wallet:
                        return await ProcessWalletRefund(order, cancellation);
                    
                    case trucki.Entities.RefundMethod.InvoiceVoid:
                        return await ProcessInvoiceVoid(order, cancellation);
                    
                    default:
                        return ApiResponseModel<bool>.Success("Refund marked for manual processing", true, 200);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for order {OrderId}", order.Id);
                return ApiResponseModel<bool>.Fail($"Error processing refund: {ex.Message}", 500);
            }
        }

        private async Task<ApiResponseModel<bool>> ProcessWalletRefund(CargoOrders order, OrderCancellation cancellation)
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
                    // Send refund notification
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

        // Remove the ProcessStripeRefund method since we're not using it

        private async Task<ApiResponseModel<bool>> ProcessInvoiceVoid(CargoOrders order, OrderCancellation cancellation)
        {
            try
            {
                var invoice = await _dbContext.Set<Invoice>()
                    .FirstOrDefaultAsync(i => i.OrderId == order.Id);

                if (invoice != null)
                {
                    invoice.Status = InvoiceStatus.Cancelled;
                    invoice.PaymentNotes = $"Invoice voided due to order cancellation. Cancellation ID: {cancellation.Id}";
                    await _dbContext.SaveChangesAsync();
                }

                return ApiResponseModel<bool>.Success("Invoice voided", true, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voiding invoice for order {OrderId}", order.Id);
                return ApiResponseModel<bool>.Fail($"Invoice void failed: {ex.Message}", 500);
            }
        }

        private List<string> GenerateWarnings(CargoOrders order, (decimal PenaltyPercentage, decimal PenaltyAmount, string Justification) penaltyInfo)
        {
            var warnings = new List<string>();

            if (penaltyInfo.PenaltyAmount > 0)
            {
                warnings.Add($"A penalty of ${penaltyInfo.PenaltyAmount:F2} ({penaltyInfo.PenaltyPercentage}%) will be deducted from your refund");
            }

            if (order.AcceptedBid?.Truck?.Driver != null)
            {
                warnings.Add("The assigned driver will be notified of this cancellation");
            }

            if (order.Status == CargoOrderStatus.DriverAcknowledged)
            {
                warnings.Add("This order has been acknowledged by the driver and may impact their schedule");
            }

            return warnings;
        }

        private async Task SendCancellationNotificationsAsync(CargoOrders order, OrderCancellation cancellation, CargoOrderStatus originalStatus)
        {
            try
            {
                // Notify assigned driver if exists
                if (!string.IsNullOrEmpty(cancellation.DriverId))
                {
                    var driver = await _dbContext.Set<Driver>()
                        .FirstOrDefaultAsync(d => d.Id == cancellation.DriverId);

                    if (driver?.UserId != null)
                    {
                        await _notificationService.SendNotificationAsync(
                            driver.UserId,
                            "Order Cancelled",
                            $"The cargo order from {order.PickupLocation} to {order.DeliveryLocation} has been cancelled by the customer",
                            new Dictionary<string, string>
                            {
                                { "orderId", order.Id },
                                { "type", "order_cancelled" },
                                { "penaltyAmount", cancellation.PenaltyAmount.ToString("F2") }
                            });

                        await _notificationEventService.NotifyOrderCancelled(
                            driver.Id,
                            order.Id,
                            order.PickupLocation,
                            order.DeliveryLocation,
                            cancellation.CancellationReason ?? "No reason provided");

                        cancellation.IsDriverNotified = true;
                    }
                }

                // Notify other bidding drivers if order was in bidding phase
                if (originalStatus == CargoOrderStatus.BiddingInProgress)
                {
                    await _notificationService.SendNotificationToTopicAsync(
                        "driver",
                        "Order No Longer Available",
                        $"A cargo order you may have bid on from {order.PickupLocation} to {order.DeliveryLocation} has been cancelled",
                        new Dictionary<string, string>
                        {
                            { "orderId", order.Id },
                            { "type", "order_cancelled_general" }
                        });
                }

                // Confirm cancellation to cargo owner
                await _notificationService.SendNotificationAsync(
                    order.CargoOwner.UserId,
                    "Order Cancellation Confirmed",
                    $"Your order from {order.PickupLocation} to {order.DeliveryLocation} has been successfully cancelled. " +
                    (cancellation.RefundAmount > 0 ? $"Refund of ${cancellation.RefundAmount:F2} is being processed." : ""),
                    new Dictionary<string, string>
                    {
                        { "orderId", order.Id },
                        { "type", "cancellation_confirmed" },
                        { "refundAmount", cancellation.RefundAmount.ToString("F2") }
                    });

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending cancellation notifications for order {OrderId}", order.Id);
                // Don't throw - notifications failing shouldn't stop the cancellation process
            }
        }
    }
}