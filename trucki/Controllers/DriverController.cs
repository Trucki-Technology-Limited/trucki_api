using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<ActionResult<ApiResponseModel<bool>>> CreateDriverAccount([FromBody] CreateDriverRequestModel model)
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

}