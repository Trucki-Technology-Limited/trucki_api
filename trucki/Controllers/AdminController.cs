using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }
        [HttpPost("CreateNewBusiness")]
        public async Task<IActionResult> CreateNewBusiness([FromBody] CreateNewBusinessRequestModel model)
        {
            var result = await _adminService.CreateNewBusiness(model);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("GetAllBusiness")]
        public async Task<ActionResult<ApiResponseModel<AllBusinessResponseModel>>> GetUserCards()
        {
            var profile = await _adminService.GetAllBusiness();
            return StatusCode(profile.StatusCode, profile);
        }
    }
}
