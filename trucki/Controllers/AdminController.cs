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
        public async Task<ActionResult<ApiResponseModel<AllBusinessResponseModel>>> GetAllBusiness()
        {
            var business = await _adminService.GetAllBusiness();
            return StatusCode(business.StatusCode, business);
        }

        [HttpPost("AddRouteToBusiness")]
        public async Task<IActionResult> AddRouteToBusiness([FromBody] AddRouteToBusinessRequestModel model)
        {
            var result = await _adminService.AddRouteToBusiness(model);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("GetBusinessById")]
        public async Task<ActionResult<ApiResponseModel<BusinessResponseModel>>> GetBusinessById(string businessId)
        {
            var business = await _adminService.GetBusinessById(businessId);
            return StatusCode(business.StatusCode, business);
        }

        [HttpPost("EditBusiness")]
        public async Task<ActionResult<ApiResponseModel<bool>>> EditBusiness([FromBody] EditBusinessRequestModel model)
        {
            var response = await _adminService.EditBusiness(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("DeleteBusiness")]
        public async Task<ActionResult<ApiResponseModel<bool>>> DeleteBusiness([FromQuery] string businessId)
        {
            var response = await _adminService.DeleteBusiness(businessId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("DisableBusiness")]
        public async Task<ActionResult<ApiResponseModel<bool>>> DisableBusiness([FromQuery] string businessId)
        {
            var response = await _adminService.DisableBusiness(businessId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("EnableBusiness")]
        public async Task<ActionResult<ApiResponseModel<bool>>> EnableBusiness([FromQuery] string businessId)
        {
            var response = await _adminService.EnableBusiness(businessId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("EditRoute")]
        public async Task<ActionResult<ApiResponseModel<bool>>> EditRoute([FromBody] EditRouteRequestModel model)
        {
            var response = await _adminService.EditRoute(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("DeleteRoute")]
        public async Task<ActionResult<ApiResponseModel<bool>>> DeleteRoute([FromQuery] string routeId)
        {
            var response = await _adminService.DeleteRoute(routeId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("AddManager")]
        public async Task<ActionResult<ApiResponseModel<bool>>> AddManager([FromBody] AddManagerRequestModel model)
        {
            var response = await _adminService.AddManager(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("AddDriver")]
        public async Task<ActionResult<ApiResponseModel<bool>>> AddDriver([FromBody] AddDriverRequestModel model)
        {
            var response = await _adminService.AddDriver(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetAllManager")]
        public async Task<ActionResult<ApiResponseModel<AllManagerResponseModel>>> GetAllManager()
        {
            var business = await _adminService.GetAllManager();
            return StatusCode(business.StatusCode, business);
        }

        [HttpPost("EditManager")]
        public async Task<ActionResult<ApiResponseModel<bool>>> EditManager([FromBody] EditManagerRequestModel model)
        {
            var response = await _adminService.EditManager(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("DeleteManager")]
        public async Task<ActionResult<ApiResponseModel<bool>>> DeleteManager([FromQuery] string managerId)
        {
            var response = await _adminService.DeactivateManager(managerId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetAllTruckOwners")]
        public async Task<ActionResult<ApiResponseModel<List<TruckOwnerResponseModel>>>> GetAllTruckOwners()
        {
            var response = await _adminService.GetAllTruckOwners();
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("DeleteTruckOwner")]
        public async Task<ActionResult<ApiResponseModel<bool>>> DeleteTruckOwner([FromQuery] string ownerId)
        {
            var response = await _adminService.DeleteTruckOwner(ownerId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("EditTruckOwner")]
        public async Task<ActionResult<ApiResponseModel<bool>>> EditTruckOwner(
            [FromForm] EditTruckOwnerRequestBody model)
        {
            var response = await _adminService.EditTruckOwner(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetTruckOwnerById")]
        public async Task<ActionResult<ApiResponseModel<TruckOwnerResponseModel>>> GetTruckOwnerById(
            [FromQuery] string ownerId)
        {
            var response = await _adminService.GetTruckOwnerById(ownerId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("CreateNewTruckOwner")]
        public async Task<ActionResult<ApiResponseModel<bool>>> CreateNewTruckOwner(
            [FromForm] AddTruckOwnerRequestBody model)
        {
            var response = await _adminService.CreateNewTruckOwner(model);
            return StatusCode(response.StatusCode, response);
        }
    }
}