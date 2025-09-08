using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BusinessController : ControllerBase
{
    private readonly IBusinessService _businessService;

    public BusinessController(IBusinessService businessService)
    {
        _businessService = businessService;
    }

    [HttpPost("CreateNewBusiness")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CreateNewBusiness([FromBody] CreateNewBusinessRequestModel model)
    {
        var result = await _businessService.CreateNewBusiness(model);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("GetAllBusiness")]
    [Authorize(Roles = "admin,manager,field officer,chiefmanager,finance")]
    public async Task<ActionResult<ApiResponseModel<PaginatedListDto<AllBusinessResponseModel>>>> GetAllBusiness(
      [FromQuery] int pageNumber = 1,
      [FromQuery] int pageSize = 10)
    {
        
        // Validate pagination parameters
        if (pageNumber < 1)
        {
            return BadRequest(new ApiResponseModel<PaginatedListDto<AllBusinessResponseModel>>
            {
                IsSuccessful = false,
                Message = "Page number must be greater than 0",
                StatusCode = 400
            });
        }

        if (pageSize < 1 || pageSize > 500)
        {
            return BadRequest(new ApiResponseModel<PaginatedListDto<AllBusinessResponseModel>>
            {
                IsSuccessful = false,
                Message = "Page size must be between 1 and 100",
                StatusCode = 400
            });
        }

        var roles = User.Claims
                      .Where(c => c.Type == ClaimTypes.Role)
                      .Select(c => c.Value)
                      .ToList();
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var business = await _businessService.GetAllBusiness(roles, userId, pageNumber, pageSize);
        return StatusCode(business.StatusCode, business);
    }

    [HttpPost("AddRouteToBusiness")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AddRouteToBusiness([FromBody] AddRouteToBusinessRequestModel model)
    {
        var result = await _businessService.AddRouteToBusiness(model);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("GetBusinessById")]
    [Authorize(Roles = "admin,manager,field officer,chiefmanager,finance")]
    public async Task<ActionResult<ApiResponseModel<BusinessResponseModel>>> GetBusinessById(string businessId)
    {
        var business = await _businessService.GetBusinessById(businessId);
        return StatusCode(business.StatusCode, business);
    }

    [HttpPost("EditBusiness")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> EditBusiness([FromBody] EditBusinessRequestModel model)
    {
        var response = await _businessService.EditBusiness(model);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("DeleteBusiness")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> DeleteBusiness([FromQuery] string businessId)
    {
        var response = await _businessService.DeleteBusiness(businessId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("DisableBusiness")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> DisableBusiness([FromQuery] string businessId)
    {
        var response = await _businessService.DisableBusiness(businessId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("EnableBusiness")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> EnableBusiness([FromQuery] string businessId)
    {
        var response = await _businessService.EnableBusiness(businessId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("EditRoute")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> EditRoute([FromBody] EditRouteRequestModel model)
    {
        var response = await _businessService.EditRoute(model);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("DeleteRoute")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> DeleteRoute([FromQuery] string routeId)
    {
        var response = await _businessService.DeleteRoute(routeId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("SearchBusinesses")]
    [Authorize(Roles = "admin,manager,chiefmanager")]
    public async Task<ActionResult<ApiResponseModel<IEnumerable<AllManagerResponseModel>>>> SearchBusinesses(string searchWords)
    {
        var response = await _businessService.SearchBusinesses(searchWords);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetRoutesByBusinessId")]
    [Authorize(Roles = "admin,manager,field officer,chiefmanager,finance")]
    public async Task<ActionResult<ApiResponseModel<List<RouteResponseModel>>>> GetRoutesByBusinessId(string businessId)
    {
        var response = await _businessService.GetRoutesByBusinessId(businessId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetBusinessGtvDashboardSummary")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<BusinessGtvDashboardSummary>>> GetBusinessGtvDashboardSummary(DateTime startDate, DateTime endDate, string businessId)
    {
        var response = await _businessService.GetBusinessGtvDashboardSummary(startDate, endDate, businessId);
        return StatusCode(response.StatusCode, response);
    }

}