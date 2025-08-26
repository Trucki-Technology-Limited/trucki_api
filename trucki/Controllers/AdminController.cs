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
        private readonly ICargoOrderService _cargoOrderService;


        public AdminController(IAdminService adminService, ICargoOwnerService cargoOwnerService,ICargoOrderService cargoOrderService)
        {
            _adminService = adminService;
            _cargoOwnerService = cargoOwnerService;
            _cargoOrderService = cargoOrderService;
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
         /// <summary>
        /// Get all cargo orders with advanced filtering, pagination, and search for admin
        /// </summary>
        [HttpGet("cargo-orders")]
        [Authorize(Roles = "admin,manager,finance")]
        public async Task<ActionResult<ApiResponseModel<PagedResponse<AdminCargoOrderResponseModel>>>> GetCargoOrders(
            [FromQuery] AdminGetCargoOrdersQueryDto query)
        {
            // Validate query parameters
            query.ValidateAndNormalize();

            var response = await _cargoOrderService.GetCargoOrdersForAdminAsync(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Get cargo order details by ID for admin
        /// </summary>
        [HttpGet("cargo-order")]
        [Authorize(Roles = "admin,manager,finance")]
        public async Task<ActionResult<ApiResponseModel<AdminCargoOrderDetailsResponseModel>>> GetCargoOrderDetails(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return BadRequest(ApiResponseModel<AdminCargoOrderDetailsResponseModel>.Fail(
                    "Order ID is required",
                    400));
            }

            var response = await _cargoOrderService.GetCargoOrderDetailsForAdminAsync(orderId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Get cargo orders statistics for admin dashboard
        /// </summary>
        [HttpGet("cargo-orders/statistics")]
        [Authorize(Roles = "admin,manager,finance")]
        public async Task<ActionResult<ApiResponseModel<AdminCargoOrderStatisticsResponseModel>>> GetCargoOrderStatistics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var response = await _cargoOrderService.GetCargoOrderStatisticsForAdminAsync(fromDate, toDate);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Flag or unflag a cargo order
        /// </summary>
        [HttpPost("cargo-orders/flag")]
        [Authorize(Roles = "admin,manager")]
        public async Task<ActionResult<ApiResponseModel<bool>>> FlagCargoOrder(
            string orderId,
            [FromBody] FlagCargoOrderRequestModel model)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return BadRequest(ApiResponseModel<bool>.Fail(
                    "Order ID is required",
                    400));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponseModel<bool>.Fail(
                    "Invalid request data",
                    400));
            }

            // Get admin user ID from claims
            var adminUserId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(adminUserId))
            {
                return Unauthorized(ApiResponseModel<bool>.Fail(
                    "User not authenticated",
                    401));
            }

            var response = await _cargoOrderService.FlagCargoOrderAsync(orderId, model.IsFlagged, model.FlagReason, adminUserId);
            return StatusCode(response.StatusCode, response);
        }
        
        /// <summary>
        /// Get cargo orders summary by status
        /// </summary>
        [HttpGet("cargo-orders/summary")]
        [Authorize(Roles = "admin,manager,finance")]
        public async Task<ActionResult<ApiResponseModel<List<CargoOrderStatusSummaryModel>>>> GetCargoOrdersSummary()
        {
            var response = await _cargoOrderService.GetCargoOrdersSummaryAsync();
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Update cargo order status (admin override)
        /// </summary>
        [HttpPatch("cargo-orders/status")]
        [Authorize(Roles = "admin,manager")]
        public async Task<ActionResult<ApiResponseModel<bool>>> UpdateCargoOrderStatus(
            string orderId,
            [FromBody] UpdateCargoOrderStatusRequestModel model)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return BadRequest(ApiResponseModel<bool>.Fail(
                    "Order ID is required",
                    400));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponseModel<bool>.Fail(
                    "Invalid request data",
                    400));
            }

            // Get admin user ID from claims
            var adminUserId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(adminUserId))
            {
                return Unauthorized(ApiResponseModel<bool>.Fail(
                    "User not authenticated",
                    401));
            }

            var response = await _cargoOrderService.UpdateCargoOrderStatusByAdminAsync(orderId, model.Status, model.Reason, adminUserId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("cargo-financial-summary")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponseModel<CargoFinancialSummaryResponse>>> GetCargoFinancialSummary(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            // Set default date range if not provided (last 30 days)
            if (startDate == default(DateTime))
                startDate = DateTime.UtcNow.AddDays(-30).Date;
    
            if (endDate == default(DateTime))
                endDate = DateTime.UtcNow.Date;

            var response = await _adminService.GetCargoFinancialSummary(startDate, endDate);
            return StatusCode(response.StatusCode, response);
        }
    }
}