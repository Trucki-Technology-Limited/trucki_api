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

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }


        [HttpGet("GetDashboardData")]
        [Authorize(Roles = "admin,finance manager")]
        public async Task<ActionResult<ApiResponseModel<DashboardSummaryResponse>>> GetDashboardData()
        {
            var response = await _adminService.GetDashBoardData();
            return StatusCode(response.StatusCode, response);
        }
     
      
        [HttpGet("GetGtvDashBoardData")]
        [Authorize(Roles = "admin,finance manager")]
        public async Task<ActionResult<ApiResponseModel<GtvDashboardSummary>>> GetGtvDashboardSummary(DateTime startDate, DateTime endDate)
        {
            var response = await _adminService.GetGtvDashBoardSummary(startDate, endDate);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("GetTruckDashboardData")]
        [Authorize(Roles = "admin,finance manager")]
        public async Task<ActionResult<ApiResponseModel<TruckDahsBoardData>>> GetTruckDashboardSummary(string truckId)
        {
            var response = await _adminService.GetTruckDashboardData(truckId);
            return StatusCode(response.StatusCode, response);
        }
        
        [HttpGet("GetOrderStatistics")]
        [Authorize(Roles = "admin,finance manager")]
        public async Task<ActionResult<ApiResponseModel<OrderStatsResponse>>> GetOrderStatistics()
        {
            var response = await _adminService.GetOrderStatistics();
            return StatusCode(response.StatusCode, response);
        }
    }
}