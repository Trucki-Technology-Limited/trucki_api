using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers;
[Route("api/[controller]")]
[ApiController]
public class DriverController : ControllerBase
{
    private readonly IDriverService _driverService;

    public DriverController(IDriverService driverService)
    {
        _driverService = driverService;
    }

    [HttpPost("AddDriver")]
    [Authorize(Roles = "admin,transporter")]
    public async Task<ActionResult<ApiResponseModel<bool>>> AddDriver([FromBody] AddDriverRequestModel model)
    {
        var response = await _driverService.AddDriver(model);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("EditDriver")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> EditDriver([FromBody] EditDriverRequestModel model)
    {
        var response = await _driverService.EditDriver(model);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("GetAllDrivers")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<AllDriverResponseModel>>> GetDrivers()
    {
        var response = await _driverService.GetAllDrivers();
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("GetDriverById")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<DriverResponseModel>>> GetDriverById(string id)
    {
        var driver = await _driverService.GetDriverById(id);

        if (driver == null)
        {
            // Return 404 with ApiResponseModel
            return NotFound(
                ApiResponseModel<DriverResponseModel>.Fail("Driver not found", StatusCodes.Status404NotFound)
            );
        }

        // Return 200 with the driver
        var response = ApiResponseModel<DriverResponseModel>.Success(
            "Driver retrieved successfully",
            driver,
            StatusCodes.Status200OK
        );

        return Ok(response);
    }
    [HttpGet("SearchDrivers")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<IEnumerable<AllDriverResponseModel>>>> SearchDrivers(string searchWords)
    {
        var response = await _driverService.SearchDrivers(searchWords);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("DeactivvateDriver")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> DeactivateDriver(string driverId)
    {
        var response = await _driverService.DeactivateDriver(driverId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetDriverProfileById")]
    [Authorize(Roles = "driver")]
    public async Task<ActionResult<ApiResponseModel<DriverProfileResponseModel>>> GetDriverProfileById()
    {

        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        var response = await _driverService.GetDriverProfileById(userId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetOrderCountByDriver")]
    [Authorize(Roles = "driver")]
    public async Task<ActionResult<ApiResponseModel<OrderCountByDriver>>> GetOrderCountByDriver(string id)
    {
        var response = await _driverService.GetOrderCountByDriver(id);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetOrderAssignedToDriver")]
    [Authorize(Roles = "driver")]
    public async Task<ActionResult<ApiResponseModel<DriverProfileResponseModel>>> GetOrderAssignedToDriver([FromQuery] string driverId)
    {
        var response = await _driverService.GetOrderAssignedToDriver(driverId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpPost("CreateDriverAccount")]
    public async Task<ActionResult<ApiResponseModel<string>>> CreateDriverAccount([FromBody] CreateDriverRequestModel model)
    {
        var response = await _driverService.CreateDriverAccount(model);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetDriversByTruckOwnerId")]
    [Authorize(Roles = "transporter")]
    public async Task<ActionResult<ApiResponseModel<AllDriverResponseModel>>> GetDriversByTruckOwnerId(string truckOwnerId)
    {
        var response = await _driverService.GetDriversByTruckOwnerId(truckOwnerId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpPost("AcceptTerms")]
    [Authorize(Roles = "driver")]
    public async Task<ActionResult<ApiResponseModel<bool>>> AcceptTermsAndConditions([FromBody] AcceptTermsRequestModel model)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        model.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var response = await _driverService.AcceptTermsAndConditions(model);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("HasAcceptedTerms")]
    [Authorize(Roles = "driver, admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> HasAcceptedLatestTerms(string driverId)
    {
        // If no driverId is provided and user is driver, use their ID
        if (string.IsNullOrEmpty(driverId) && User.IsInRole("driver"))
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

        }

        var response = await _driverService.HasAcceptedLatestTerms(driverId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("TermsAcceptanceHistory")]
    [Authorize(Roles = "admin,driver")]
    public async Task<ActionResult<ApiResponseModel<List<TermsAcceptanceRecordDto>>>> GetTermsAcceptanceHistory(string driverId)
    {
        var response = await _driverService.GetTermsAcceptanceHistory(driverId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpPost("UpdateProfilePhoto")]
    [Authorize(Roles = "driver")]
    public async Task<ActionResult<ApiResponseModel<bool>>> UpdateProfilePhoto([FromBody] UpdateDriverProfilePhotoRequestModel model)
    {
        // For security, verify the driver is updating their own profile,
        // or an admin is making the change
        // var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // var userRole = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

        // if (string.IsNullOrEmpty(userId))
        // {
        //     return Unauthorized(ApiResponseModel<bool>.Fail("Unauthorized", StatusCodes.Status401Unauthorized));
        // }

        // If user is a driver (not admin), ensure they're updating their own profile
        // if (userRole == "driver")
        // {
        //     var driverProfile = await _driverService.GetDriverProfileById(userId);
        //     if (driverProfile.Data?.Id != model.DriverId)
        //     {
        //         return Forbid();
        //     }
        // }

        var response = await _driverService.UpdateDriverProfilePhoto(model);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("SetDriverOnboardingInReview")]
    [Authorize(Roles = "driver")]
    public async Task<ActionResult<ApiResponseModel<bool>>> SetDriverOnboardingInReview(string driverId)
    {
        if (string.IsNullOrEmpty(driverId))
        {
            return BadRequest(
                ApiResponseModel<bool>.Fail(
                    "Driver ID is required",
                    StatusCodes.Status400BadRequest
                )
            );
        }

        // Set the driver onboarding status to InReview
        var response = await _driverService.UpdateDriverOnboardingStatus(driverId, DriverOnboardingStatus.OnboardingInReview);

        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetAllDriversPaginated")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<PaginatedListDto<AllDriverResponseModel>>>> GetAllDriversPaginated(
        [FromQuery] GetAllDriversRequestModel request)
    {
        // Validate the request
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponseModel<PaginatedListDto<AllDriverResponseModel>>
            {
                IsSuccessful = false,
                Message = "Invalid request parameters",
                StatusCode = 400
            });
        }

        var response = await _driverService.GetAllDriversPaginated(request);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("GetDriversSummary")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<AdminDriverSummaryResponseModel>>> GetDriversSummary()
    {
        var response = await _driverService.GetAdminDriversSummary();
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("UpdateDotNumber")]
    [Authorize(Roles = "driver")]
    public async Task<ActionResult<ApiResponseModel<bool>>> UpdateDotNumber([FromBody] UpdateDotNumberRequestModel model)
    {
        // Get the current user ID from the token
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // For security, verify the driver is updating their own DOT number
        var driverProfile = await _driverService.GetDriverProfileById(userId);
        if (driverProfile.Data?.Id != model.DriverId)
        {
            return ApiResponseModel<bool>.Fail("You can only update your own DOT number", StatusCodes.Status403Forbidden);
        }

        var response = await _driverService.UpdateDotNumber(model);
        return StatusCode(response.StatusCode, response);
    }
}