using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers;
[Route("api/[controller]")]
[ApiController]
public class DriverController: ControllerBase
{
    private readonly IDriverService _driverService;

    public DriverController(IDriverService driverService)
    {
        _driverService = driverService;
    }
    
    [HttpPost("AddDriver")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> AddDriver([FromForm] AddDriverRequestModel model)
    {
        var response = await _driverService.AddDriver(model);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("EditDriver")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> EditDriver([FromForm] EditDriverRequestModel model)
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
        var response = await _driverService.GetDriverById(id);
        return StatusCode(response.StatusCode, response);
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


}