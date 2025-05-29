using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;
using trucki.Services;

namespace trucki.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderCancellationController : ControllerBase
    {
        private readonly IOrderCancellationService _cancellationService;
        private readonly ICargoOwnerService _cargoOwnerService;

        public OrderCancellationController(
            IOrderCancellationService cancellationService,
            ICargoOwnerService cargoOwnerService)
        {
            _cancellationService = cancellationService;
            _cargoOwnerService = cargoOwnerService;
        }

        /// <summary>
        /// Get cancellation preview showing penalty and refund details
        /// </summary>
        [HttpGet("preview/{orderId}")]
        [Authorize(Roles = "cargo owner")]
        public async Task<ActionResult<ApiResponseModel<CancellationPreviewResponseModel>>> GetCancellationPreview(string orderId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Get cargo owner profile
                var profileResult = await _cargoOwnerService.GetCargoOwnerProfile(userId);
                if (!profileResult.IsSuccessful || profileResult.Data == null)
                {
                    return StatusCode(profileResult.StatusCode,
                        ApiResponseModel<CancellationPreviewResponseModel>.Fail(profileResult.Message, profileResult.StatusCode));
                }

                var result = await _cancellationService.GetCancellationPreviewAsync(orderId, profileResult.Data.Id);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseModel<CancellationPreviewResponseModel>.Fail(
                    $"Error getting cancellation preview: {ex.Message}", 500));
            }
        }

        /// <summary>
        /// Cancel an order with refund processing
        /// </summary>
        [HttpPost("cancel")]
        [Authorize(Roles = "cargo owner")]
        public async Task<ActionResult<ApiResponseModel<OrderCancellationResponseModel>>> CancelOrder([FromBody] CancelOrderRequestDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Get cargo owner profile
                var profileResult = await _cargoOwnerService.GetCargoOwnerProfile(userId);
                if (!profileResult.IsSuccessful || profileResult.Data == null)
                {
                    return StatusCode(profileResult.StatusCode,
                        ApiResponseModel<OrderCancellationResponseModel>.Fail(profileResult.Message, profileResult.StatusCode));
                }

                // Set the cargo owner ID from the authenticated user
                request.CargoOwnerId = profileResult.Data.Id;

                var result = await _cancellationService.CancelOrderAsync(request);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseModel<OrderCancellationResponseModel>.Fail(
                    $"Error cancelling order: {ex.Message}", 500));
            }
        }

        /// <summary>
        /// Get cancellation history for the authenticated cargo owner
        /// </summary>
        [HttpGet("history")]
        [Authorize(Roles = "cargo owner")]
        public async Task<ActionResult<ApiResponseModel<IEnumerable<OrderCancellation>>>> GetCancellationHistory()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Get cargo owner profile
                var profileResult = await _cargoOwnerService.GetCargoOwnerProfile(userId);
                if (!profileResult.IsSuccessful || profileResult.Data == null)
                {
                    return StatusCode(profileResult.StatusCode,
                        ApiResponseModel<IEnumerable<OrderCancellation>>.Fail(profileResult.Message, profileResult.StatusCode));
                }

                var result = await _cancellationService.GetCancellationHistoryAsync(profileResult.Data.Id);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseModel<IEnumerable<OrderCancellation>>.Fail(
                    $"Error retrieving cancellation history: {ex.Message}", 500));
            }
        }
        /// <summary>
        /// Check if an order can be cancelled (quick endpoint for UI)
        /// </summary>
        [HttpGet("can-cancel/{orderId}")]
        [Authorize(Roles = "cargo owner")]
        public async Task<ActionResult<ApiResponseModel<bool>>> CanCancelOrder(string orderId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Get cargo owner profile
                var profileResult = await _cargoOwnerService.GetCargoOwnerProfile(userId);
                if (!profileResult.IsSuccessful || profileResult.Data == null)
                {
                    return StatusCode(profileResult.StatusCode,
                        ApiResponseModel<bool>.Fail(profileResult.Message, profileResult.StatusCode));
                }

                var previewResult = await _cancellationService.GetCancellationPreviewAsync(orderId, profileResult.Data.Id);

                if (!previewResult.IsSuccessful)
                {
                    return StatusCode(previewResult.StatusCode,
                        ApiResponseModel<bool>.Fail(previewResult.Message, previewResult.StatusCode));
                }

                return Ok(ApiResponseModel<bool>.Success(
                    previewResult.Data.CanCancel ? "Order can be cancelled" : "Order cannot be cancelled",
                    previewResult.Data.CanCancel,
                    200));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseModel<bool>.Fail(
                    $"Error checking cancellation eligibility: {ex.Message}", 500));
            }
        }
    }
}