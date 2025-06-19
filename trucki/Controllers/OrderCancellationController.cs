using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;
using trucki.Services;

namespace trucki.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "cargo owner")]
    public class OrderCancellationController : ControllerBase
    {
        private readonly IOrderCancellationService _orderCancellationService;
        private readonly IStripeService _stripeService;
        private readonly TruckiDBContext _dbContext;

        public OrderCancellationController(
            IOrderCancellationService orderCancellationService,
            IStripeService stripeService,
            TruckiDBContext dbContext)
        {
            _orderCancellationService = orderCancellationService;
            _stripeService = stripeService;
            _dbContext = dbContext;
        }

        [HttpGet("preview/{orderId}")]
        public async Task<ActionResult<ApiResponseModel<CancellationPreviewResponseModel>>> GetCancellationPreview(string orderId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Get cargo owner ID for this user
                var cargoOwner = await _dbContext.Set<CargoOwner>()
                    .FirstOrDefaultAsync(co => co.UserId == userId);

                if (cargoOwner == null)
                {
                    return BadRequest(ApiResponseModel<CancellationPreviewResponseModel>.Fail("Cargo owner not found", 404));
                }

                var result = await _orderCancellationService.GetCancellationPreviewAsync(orderId, cargoOwner.Id);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseModel<CancellationPreviewResponseModel>.Fail($"Error: {ex.Message}", 500));
            }
        }

        [HttpPost("cancel")]
        public async Task<ActionResult<ApiResponseModel<OrderCancellationResponseModel>>> CancelOrder([FromBody] CancelOrderRequestDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Get cargo owner ID for this user
                var cargoOwner = await _dbContext.Set<CargoOwner>()
                    .FirstOrDefaultAsync(co => co.UserId == userId);

                if (cargoOwner == null)
                {
                    return BadRequest(ApiResponseModel<OrderCancellationResponseModel>.Fail("Cargo owner not found", 404));
                }

                // Verify the cargo owner ID matches
                if (request.CargoOwnerId != cargoOwner.Id)
                {
                    return BadRequest(ApiResponseModel<OrderCancellationResponseModel>.Fail("Unauthorized", 403));
                }

                var result = await _orderCancellationService.CancelOrderAsync(request);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseModel<OrderCancellationResponseModel>.Fail($"Error: {ex.Message}", 500));
            }
        }

        [HttpPost("create-cancellation-fee-payment-intent")]
        public async Task<ActionResult<ApiResponseModel<StripePaymentResponse>>> CreateCancellationFeePaymentIntent([FromBody] CreateCancellationFeePaymentIntentDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Get cargo owner and verify they're a broker
                var cargoOwner = await _dbContext.Set<CargoOwner>()
                    .FirstOrDefaultAsync(co => co.UserId == userId && co.Id == request.CargoOwnerId);

                if (cargoOwner == null)
                {
                    return BadRequest(ApiResponseModel<StripePaymentResponse>.Fail("Cargo owner not found", 404));
                }

                if (cargoOwner.OwnerType != CargoOwnerType.Broker)
                {
                    return BadRequest(ApiResponseModel<StripePaymentResponse>.Fail("This endpoint is only available for brokers", 400));
                }

                // Verify the order exists and belongs to this broker
                var order = await _dbContext.Set<CargoOrders>()
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.CargoOwnerId == request.CargoOwnerId);

                if (order == null)
                {
                    return NotFound(ApiResponseModel<StripePaymentResponse>.Fail("Order not found", 404));
                }

                // Create payment intent for cancellation fee
                var paymentResponse = await _stripeService.CreatePaymentIntent(
                    $"cancellation-{request.OrderId}",
                    request.PenaltyAmount,
                    request.Currency);

                paymentResponse.OrderId = request.OrderId;

                return Ok(ApiResponseModel<StripePaymentResponse>.Success(
                    "Cancellation fee payment intent created successfully",
                    paymentResponse,
                    200));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseModel<StripePaymentResponse>.Fail($"Error: {ex.Message}", 500));
            }
        }

        [HttpPost("confirm-cancellation-fee-payment")]
        public async Task<ActionResult<ApiResponseModel<OrderCancellationResponseModel>>> ConfirmCancellationFeePayment([FromBody] ConfirmCancellationFeePaymentDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Verify payment with Stripe first
                var isSuccessful = await _stripeService.VerifyPaymentStatus(request.PaymentIntentId);
                if (!isSuccessful)
                {
                    return BadRequest(ApiResponseModel<OrderCancellationResponseModel>.Fail("Payment verification failed", 400));
                }

                // Get cargo owner and verify they're a broker
                var cargoOwner = await _dbContext.Set<CargoOwner>()
                    .FirstOrDefaultAsync(co => co.UserId == userId && co.Id == request.CargoOwnerId);

                if (cargoOwner == null)
                {
                    return BadRequest(ApiResponseModel<OrderCancellationResponseModel>.Fail("Cargo owner not found", 404));
                }

                if (cargoOwner.OwnerType != CargoOwnerType.Broker)
                {
                    return BadRequest(ApiResponseModel<OrderCancellationResponseModel>.Fail("This endpoint is only available for brokers", 400));
                }

                // Create cancellation request with payment intent ID
                var cancelRequest = new CancelOrderRequestDto
                {
                    OrderId = request.OrderId,
                    CargoOwnerId = request.CargoOwnerId,
                    CancellationReason = request.CancellationReason,
                    CancellationFeePaymentIntentId = request.PaymentIntentId
                };

                var result = await _orderCancellationService.CancelOrderAsync(cancelRequest);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseModel<OrderCancellationResponseModel>.Fail($"Error: {ex.Message}", 500));
            }
        }

        [HttpPost("process-refund")]
        [Authorize(Roles = "admin")] // Only admins can manually process refunds
        public async Task<ActionResult<ApiResponseModel<bool>>> ProcessCancellationRefund([FromBody] ProcessCancellationRefundDto request)
        {
            try
            {
                var result = await _orderCancellationService.ProcessCancellationRefundAsync(request);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseModel<bool>.Fail($"Error: {ex.Message}", 500));
            }
        }
    }
}