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
            var cargoOwnerId = User.FindFirst("CargoOwnerId")?.Value;
            if (string.IsNullOrEmpty(cargoOwnerId))
            {
                return BadRequest(new ApiResponseModel<bool>
                {
                    IsSuccessful = false,
                    Message = "Only cargo owners can submit ratings",
                    StatusCode = 400
                });
            }

            var result = await _ratingRepository.SubmitRatingAsync(model, cargoOwnerId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get rating summary for a driver (Average rating, total ratings, breakdown)
        /// </summary>
        [HttpGet("driver/summary")]
        public async Task<ActionResult<ApiResponseModel<DriverRatingSummaryModel>>> GetDriverRatingSummary(string driverId)
        {
            var result = await _ratingRepository.GetDriverRatingSummaryAsync(driverId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get all ratings and reviews for a driver (Admin only)
        /// </summary>
        [HttpGet("driver/all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseModel<IEnumerable<DriverRatingResponseModel>>>> GetDriverRatings(string driverId)
        {
            var result = await _ratingRepository.GetDriverRatingsAsync(driverId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get rating for a specific order (Driver can see their rating for an order)
        /// </summary>
        [HttpGet("order")]
        public async Task<ActionResult<ApiResponseModel<DriverRatingResponseModel>>> GetRatingByOrderId(string orderId)
        {
            var result = await _ratingRepository.GetRatingByOrderIdAsync(orderId);
            return StatusCode(result.StatusCode, result);
        }
    }
}