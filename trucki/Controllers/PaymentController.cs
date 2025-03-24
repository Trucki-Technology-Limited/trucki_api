using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Interfaces.IServices;
using trucki.Models.ResponseModels;

namespace trucki.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IStripeService _stripeService;
    private readonly ICargoOrderService _cargoOrderService;

    public PaymentController(IStripeService stripeService, ICargoOrderService cargoOrderService)
    {
        _stripeService = stripeService;
        _cargoOrderService = cargoOrderService;
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
}