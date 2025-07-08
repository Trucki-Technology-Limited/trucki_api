using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using trucki.Interfaces.IRepository;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RatingController : ControllerBase
    {
        private readonly IDriverRatingRepository _ratingRepository;

        public RatingController(IDriverRatingRepository ratingRepository)
        {
            _ratingRepository = ratingRepository;
        }

        /// <summary>
        /// Submit rating for a driver after order completion (Cargo Owner only)
        /// </summary>
        [HttpPost("submit")]
        public async Task<ActionResult<ApiResponseModel<bool>>> SubmitRating([FromBody] SubmitRatingDto model)
        {
           
            var result = await _ratingRepository.SubmitRatingAsync(model, model.CargoOwnerId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get rating summary for a driver (Average rating, total ratings, breakdown)
        /// </summary>
        [HttpGet("driver/{driverId}/summary")]
        public async Task<ActionResult<ApiResponseModel<DriverRatingSummaryModel>>> GetDriverRatingSummary(string driverId)
        {
            var result = await _ratingRepository.GetDriverRatingSummaryAsync(driverId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get all ratings and reviews for a driver (Admin only)
        /// </summary>
        [HttpGet("driver/{driverId}/all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseModel<IEnumerable<DriverRatingResponseModel>>>> GetDriverRatings(string driverId)
        {
            var result = await _ratingRepository.GetDriverRatingsAsync(driverId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get rating for a specific order (Driver can see their rating for an order)
        /// </summary>
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<ApiResponseModel<DriverRatingResponseModel>>> GetRatingByOrderId(string orderId)
        {
            var result = await _ratingRepository.GetRatingByOrderIdAsync(orderId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Check if cargo owner has already rated an order
        /// </summary>
        [HttpGet("order/has-rated")]
        public async Task<ActionResult<ApiResponseModel<bool>>> HasCargoOwnerRatedOrder(string orderId,string cargoOwnerId)
        {
            var result = await _ratingRepository.HasCargoOwnerRatedOrderAsync(orderId, cargoOwnerId);
            return StatusCode(result.StatusCode, result);
        }
    }
}