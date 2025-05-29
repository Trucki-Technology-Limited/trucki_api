using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using trucki.Interfaces.IServices;
using trucki.Models.ResponseModels;
using trucki.Models.RequestModel;
using trucki.DatabaseContext;
using trucki.Entities;

namespace trucki.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IStripeService _stripeService;
    private readonly ICargoOrderService _cargoOrderService;
    private readonly IWalletService _walletService;
    private readonly TruckiDBContext _dbContext;

    public PaymentController(
        IStripeService stripeService, 
        ICargoOrderService cargoOrderService,
        IWalletService walletService,
        TruckiDBContext dbContext)
    {
        _stripeService = stripeService;
        _cargoOrderService = cargoOrderService;
        _walletService = walletService;
        _dbContext = dbContext;
    }

    [HttpPost("create-payment-intent")]
    [Authorize(Roles = "cargo owner")]
    public async Task<ActionResult<ApiResponseModel<StripePaymentResponse>>> CreatePaymentIntent([FromBody] CreatePaymentIntentDto model)
    {
        try
        {
            var response = await _stripeService.CreatePaymentIntent(model.OrderId, model.Amount);
            return Ok(ApiResponseModel<StripePaymentResponse>.Success("Payment intent created", response, 200));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseModel<StripePaymentResponse>.Fail($"Error creating payment intent: {ex.Message}", 500));
        }
    }

    [HttpPost("confirm-payment")]
    [Authorize(Roles = "cargo owner")]
    public async Task<ActionResult<ApiResponseModel<bool>>> ConfirmPayment([FromBody] ConfirmPaymentDto model)
    {
        try
        {
            var isSuccessful = await _stripeService.VerifyPaymentStatus(model.PaymentIntentId);
            if (isSuccessful)
            {
                // Update order status to reflect payment
                var result = await _cargoOrderService.UpdateOrderPaymentStatusAsync(model.OrderId, model.PaymentIntentId);
                return StatusCode(result.StatusCode, result);
            }
            
            return BadRequest(ApiResponseModel<bool>.Fail("Payment verification failed", 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseModel<bool>.Fail($"Error confirming payment: {ex.Message}", 500));
        }
    }

    [HttpPost("broker/pay-now")]
    [Authorize(Roles = "cargo owner")]
    public async Task<ActionResult<ApiResponseModel<StripePaymentResponse>>> BrokerPayNow([FromBody] BrokerPayNowDto model)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Get the invoice with related order and cargo owner
            var invoice = await _dbContext.Set<Invoice>()
                .Include(i => i.Order)
                    .ThenInclude(o => o.CargoOwner)
                .FirstOrDefaultAsync(i => i.Id == model.InvoiceId);

            if (invoice == null)
            {
                return NotFound(ApiResponseModel<StripePaymentResponse>.Fail("Invoice not found", 404));
            }

            // Verify this user owns the invoice
            if (invoice.Order.CargoOwner.UserId != userId)
            {
                return BadRequest(ApiResponseModel<StripePaymentResponse>.Fail(
                    "Invoice does not belong to this user", 400));
            }

            // Verify this is a broker
            if (invoice.Order.CargoOwner.OwnerType != CargoOwnerType.Broker)
            {
                return BadRequest(ApiResponseModel<StripePaymentResponse>.Fail(
                    "This endpoint is only available for brokers", 400));
            }

            // Verify invoice is pending payment
            if (invoice.Status != InvoiceStatus.Pending && 
                invoice.Status != InvoiceStatus.Overdue)
            {
                return BadRequest(ApiResponseModel<StripePaymentResponse>.Fail(
                    "Invoice is not in a payable state", 400));
            }

            // Get wallet balance
            var walletBalance = await _walletService.GetWalletBalance(invoice.Order.CargoOwnerId);
            var totalAmount = invoice.TotalAmount;

            // Determine payment breakdown
            var walletPaymentAmount = Math.Min(walletBalance, totalAmount);
            var remainingPaymentAmount = totalAmount - walletPaymentAmount;

            // If payment is fully covered by wallet
            if (remainingPaymentAmount <= 0)
            {
                // Process wallet payment directly
                var walletResult = await _walletService.DeductFundsFromWallet(
                    invoice.Order.CargoOwnerId,
                    walletPaymentAmount,
                    $"Payment for Invoice #{invoice.InvoiceNumber}",
                    WalletTransactionType.Payment,
                    invoice.OrderId);

                if (!walletResult.IsSuccessful)
                {
                    return StatusCode(
                        walletResult.StatusCode,
                        ApiResponseModel<StripePaymentResponse>.Fail(walletResult.Message, walletResult.StatusCode));
                }

                // Update invoice status
                invoice.Status = InvoiceStatus.Paid;
                invoice.PaymentApprovedAt = DateTime.UtcNow;
                invoice.PaymentNotes = "Paid via wallet";
                await _dbContext.SaveChangesAsync();

                // Return response without Stripe payment intent
                var walletResponse = new StripePaymentResponse
                {
                    OrderId = invoice.OrderId,
                    Amount = totalAmount,
                    Status = "succeeded",
                    PaymentBreakdown = new PaymentBreakdown
                    {
                        BidAmount = invoice.SubTotal,
                        SystemFee = invoice.SystemFee,
                        Tax = invoice.Tax,
                        TotalAmount = totalAmount,
                        WalletAmount = walletPaymentAmount,
                        RemainingAmount = 0
                    }
                };

                return Ok(ApiResponseModel<StripePaymentResponse>.Success(
                    "Payment processed from wallet balance",
                    walletResponse,
                    200));
            }
            else
            {
                // Create Stripe payment intent for remaining amount
                var paymentResponse = await _stripeService.CreatePaymentIntent(
                    invoice.OrderId,
                    remainingPaymentAmount,
                    "usd");

                // Store payment info for later processing
                invoice.PaymentNotes = $"Pending: Wallet=${walletPaymentAmount:F2}, Stripe=${remainingPaymentAmount:F2}, PaymentIntent={paymentResponse.PaymentIntentId}";
                await _dbContext.SaveChangesAsync();

                // Add payment breakdown
                paymentResponse.PaymentBreakdown = new PaymentBreakdown
                {
                    BidAmount = invoice.SubTotal,
                    SystemFee = invoice.SystemFee,
                    Tax = invoice.Tax,
                    TotalAmount = totalAmount,
                    WalletAmount = walletPaymentAmount,
                    RemainingAmount = remainingPaymentAmount
                };

                return Ok(ApiResponseModel<StripePaymentResponse>.Success(
                    walletPaymentAmount > 0
                        ? $"${walletPaymentAmount:F2} applied from wallet balance. Please complete remaining payment."
                        : "Please complete payment",
                    paymentResponse,
                    200));
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseModel<StripePaymentResponse>.Fail(
                $"Error processing broker payment: {ex.Message}", 500));
        }
    }

    [HttpPost("broker/confirm-payment")]
    [Authorize(Roles = "cargo owner")]
    public async Task<ActionResult<ApiResponseModel<bool>>> BrokerConfirmPayment([FromBody] BrokerConfirmPaymentDto model)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Verify payment with Stripe
            var isSuccessful = await _stripeService.VerifyPaymentStatus(model.PaymentIntentId);
            if (!isSuccessful)
            {
                return BadRequest(ApiResponseModel<bool>.Fail("Payment verification failed", 400));
            }

            // Get the invoice with related order
            var invoice = await _dbContext.Set<Invoice>()
                .Include(i => i.Order)
                    .ThenInclude(o => o.CargoOwner)
                .FirstOrDefaultAsync(i => i.Id == model.InvoiceId);

            if (invoice == null)
            {
                return NotFound(ApiResponseModel<bool>.Fail("Invoice not found", 404));
            }

            // Verify the invoice belongs to the requesting user
            if (invoice.Order.CargoOwner.UserId != userId)
            {
                return BadRequest(ApiResponseModel<bool>.Fail("Unauthorized", 403));
            }

            // Extract wallet amount from payment notes if it exists
            decimal walletAmount = 0;
            if (!string.IsNullOrEmpty(invoice.PaymentNotes) && invoice.PaymentNotes.Contains("Wallet="))
            {
                var walletPart = invoice.PaymentNotes.Split("Wallet=")[1].Split(",")[0].Trim().Replace("$", "");
                decimal.TryParse(walletPart, out walletAmount);
            }

            // Process wallet payment if there was one
            if (walletAmount > 0)
            {
                var walletResult = await _walletService.DeductFundsFromWallet(
                    invoice.Order.CargoOwnerId,
                    walletAmount,
                    $"Payment for Invoice #{invoice.InvoiceNumber}",
                    WalletTransactionType.Payment,
                    invoice.OrderId);

                if (!walletResult.IsSuccessful)
                {
                    return StatusCode(
                        walletResult.StatusCode,
                        ApiResponseModel<bool>.Fail($"Error processing wallet payment: {walletResult.Message}", walletResult.StatusCode));
                }
            }

            // Mark invoice as paid
            invoice.Status = InvoiceStatus.Paid;
            invoice.PaymentApprovedAt = DateTime.UtcNow;
            invoice.PaymentNotes = $"Paid: Wallet=${walletAmount:F2}, Stripe confirmed, PaymentIntent={model.PaymentIntentId}";

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponseModel<bool>.Success("Broker payment confirmed successfully", true, 200));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseModel<bool>.Fail(
                $"Error confirming broker payment: {ex.Message}", 500));
        }
    }
}