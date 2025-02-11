using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CargoOwnerController : ControllerBase
    {
        private readonly ICargoOwnerService _cargoOwnerService;

        public CargoOwnerController(ICargoOwnerService cargoOwnerService)
        {
            _cargoOwnerService = cargoOwnerService;
        }


        [HttpPost("create")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseModel<string>>> CreateCargoOwnerAccount([FromBody] CreateCargoOwnerRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseModel<string>
                {
                    IsSuccessful = false,
                    Message = "Invalid input data",
                    StatusCode = 400
                });
            }

            var result = await _cargoOwnerService.CreateCargoOwnerAccount(model);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPut("edit")]
        [Authorize(Roles = "cargo owner")]
        public async Task<ActionResult<ApiResponseModel<bool>>> EditCargoOwnerProfile([FromBody] EditCargoOwnerRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseModel<bool>
                {
                    IsSuccessful = false,
                    Message = "Invalid input data",
                    StatusCode = 400
                });
            }

            var result = await _cargoOwnerService.EditCargoOwnerProfile(model);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("deactivate/{cargoOwnerId}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponseModel<bool>>> DeactivateCargoOwner(string cargoOwnerId)
        {
            if (string.IsNullOrEmpty(cargoOwnerId))
            {
                return BadRequest(new ApiResponseModel<bool>
                {
                    IsSuccessful = false,
                    Message = "Cargo owner ID is required",
                    StatusCode = 400
                });
            }

            var result = await _cargoOwnerService.DeactivateCargoOwner(cargoOwnerId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("GetCargoOwnerOrders")]
        [Authorize(Roles = "cargo owner")]
        public async Task<ActionResult<ApiResponseModel<List<CargoOrderResponseModel>>>> GetCargoOwnerOrders(
    [FromQuery] string cargoOwnerId,
    [FromQuery] CargoOrderStatus? status = null,
    [FromQuery] string sortBy = "date",
    [FromQuery] bool sortDescending = true)
        {
            if (string.IsNullOrEmpty(cargoOwnerId))
            {
                return BadRequest(new ApiResponseModel<List<CargoOrderResponseModel>>
                {
                    IsSuccessful = false,
                    Message = "Cargo owner ID is required",
                    StatusCode = 400
                });
            }

            // Validate sort parameter
            if (!string.IsNullOrEmpty(sortBy) && !new[] { "date", "status" }.Contains(sortBy.ToLower()))
            {
                return BadRequest(new ApiResponseModel<List<CargoOrderResponseModel>>
                {
                    IsSuccessful = false,
                    Message = "Invalid sort parameter. Use 'date' or 'status'",
                    StatusCode = 400
                });
            }

            var query = new GetCargoOwnerOrdersQueryDto
            {
                CargoOwnerId = cargoOwnerId,
                Status = status,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _cargoOwnerService.GetCargoOwnerOrders(query);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("profile")]
        [Authorize(Roles = "cargo owner")]
        public async Task<ActionResult<ApiResponseModel<CargoOwnerProfileResponseModel>>> GetCargoOwnerProfile()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var result = await _cargoOwnerService.GetCargoOwnerProfile(userId);
            return StatusCode(result.StatusCode, result);
        }
    }
}