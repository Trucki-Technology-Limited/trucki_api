using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ICargoOwnerService _cargoOwnerService;


        public AdminController(IAdminService adminService, ICargoOwnerService cargoOwnerService)
        {
            _adminService = adminService;
            _cargoOwnerService = cargoOwnerService;
        }

        [HttpGet("GetDashboardData")]
        [Authorize(Roles = "admin,finance")]
        public async Task<ActionResult<ApiResponseModel<DashboardSummaryResponse>>> GetDashboardData()
        {
            var response = await _adminService.GetDashBoardData();
            return StatusCode(response.StatusCode, response);
        }


        [HttpGet("GetGtvDashBoardData")]
        [Authorize(Roles = "admin,finance")]
        public async Task<ActionResult<ApiResponseModel<GtvDashboardSummary>>> GetGtvDashboardSummary(DateTime startDate, DateTime endDate)
        {
            var response = await _adminService.GetGtvDashBoardSummary(startDate, endDate);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("GetTruckDashboardData")]
        [Authorize(Roles = "admin,finance")]
        public async Task<ActionResult<ApiResponseModel<TruckDahsBoardData>>> GetTruckDashboardSummary(string truckId)
        {
            var response = await _adminService.GetTruckDashboardData(truckId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetOrderStatistics")]
        [Authorize(Roles = "admin,finance")]
        public async Task<ActionResult<ApiResponseModel<OrderStatsResponse>>> GetOrderStatistics(DateTime startDate, DateTime endDate)
        {
            var response = await _adminService.GetOrderStatistics(startDate, endDate);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("cargo-owners")]
        [Authorize(Roles = "admin,manager,finance")]
        public async Task<ActionResult<ApiResponseModel<PagedResponse<AdminCargoOwnerResponseModel>>>> GetCargoOwners(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? searchQuery = null)
        {
            // Validate pagination parameters
            if (pageNumber < 1)
            {
                return BadRequest(ApiResponseModel<PagedResponse<AdminCargoOwnerResponseModel>>.Fail(
                    "Page number must be greater than 0",
                    400));
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(ApiResponseModel<PagedResponse<AdminCargoOwnerResponseModel>>.Fail(
                    "Page size must be between 1 and 100",
                    400));
            }

            var response = await _cargoOwnerService.GetCargoOwnersWithPagination(pageNumber, pageSize, searchQuery);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("cargo-owners/{cargoOwnerId}")]
        [Authorize(Roles = "admin,manager,finance")]
        public async Task<ActionResult<ApiResponseModel<AdminCargoOwnerDetailsResponseModel>>> GetCargoOwnerDetails(string cargoOwnerId)
        {
            if (string.IsNullOrWhiteSpace(cargoOwnerId))
            {
                return BadRequest(ApiResponseModel<AdminCargoOwnerDetailsResponseModel>.Fail(
                    "Cargo owner ID is required",
                    400));
            }

            var response = await _cargoOwnerService.GetCargoOwnerDetailsForAdmin(cargoOwnerId);
            return StatusCode(response.StatusCode, response);
        }
    }
}