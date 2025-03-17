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
public class TruckController : ControllerBase
{
    private readonly ITruckService _truckService;

    public TruckController(ITruckService truckService)
    {
        _truckService = truckService;
    }

    [HttpPost("AddNewTruck")]
    [Authorize(Roles = "admin,transporter")]
    public async Task<ActionResult<ApiResponseModel<string>>> AddNewTruck([FromBody] AddTruckRequestModel model)
    {
        var response = await _truckService.AddNewTruck(model);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("AddDriverOwnedTruck")]
    [Authorize(Roles = "driver")]
    public async Task<ActionResult<ApiResponseModel<string>>> AddDriverOwnedTruck([FromBody] DriverAddTruckRequestModel model)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var response = await _truckService.AddDriverOwnedTruck(model);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("EditTruck")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> EditTruck([FromBody] EditTruckRequestModel model)
    {
        var response = await _truckService.EditTruck(model);
        return StatusCode(response.StatusCode, response);
    }
    [HttpPost("DeleteTruck")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<string>>> DeleteTruck(string truckId)
    {
        var response = await _truckService.DeleteTruck(truckId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetTruckById")]
    [Authorize(Roles = "admin,transporter")]
    public async Task<ActionResult<ApiResponseModel<AllTruckResponseModel>>> GetTruckById(string truckId)
    {
        var response = await _truckService.GetTruckById(truckId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("SearchTrucks")]
    [Authorize(Roles = "admin,manager,chiefmanager")]
    public async Task<ActionResult<ApiResponseModel<IEnumerable<AllTruckResponseModel>>>> SearchTruck(string? searchWords)
    {
        var response = await _truckService.SearchTruck(searchWords);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetAllTrucks")]
    [Authorize(Roles = "admin,manager,field officer,chiefmanager")]
    public async Task<ActionResult<ApiResponseModel<List<AllTruckResponseModel>>>> GetAllTrucks()
    {
        var response = await _truckService.GetAllTrucks();
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetTruckDocuments")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<IEnumerable<string>>>> GetTruckDocuments(string truckId)
    {
        var response = await _truckService.GetTruckDocuments(truckId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpPost("AssignDriverToTruck")]
    [Authorize(Roles = "admin,transporter")]
    public async Task<ActionResult<ApiResponseModel<bool>>> AssignDriverToTruck(AssignDriverToTruckRequestModel model)
    {
        var response = await _truckService.AssignDriverToTruck(model);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("UpdateTruckStatus")]
    [Authorize(Roles = "admin,manager,chiefmanager,transporter")]
    public async Task<ActionResult<ApiResponseModel<string>>> UpdateTruckStatus(string truckId, UpdateTruckStatusRequestModel model)
    {
        var response = await _truckService.UpdateTruckStatus(truckId, model);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetTrucksByOwnersId")]
    [Authorize(Roles = "transporter")]
    public async Task<ActionResult<ApiResponseModel<List<AllTruckResponseModel>>>> GetTrucksByOwnersId(string ownersId)
    {
        var response = await _truckService.GetTrucksByOwnersId(ownersId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetTruckStatusCountByOwnerId")]
    [Authorize(Roles = "transporter")]
    public async Task<ActionResult<ApiResponseModel<TruckStatusCountResponseModel>>> GetTruckStatusCountByOwnerId(string ownersId)
    {
        var response = await _truckService.GetTruckStatusCountByOwnerId(ownersId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpPost("UpdateTruckApprovalStatus")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<string>>> UpdateTruckApprovalStatus(string truckId, ApprovalStatus approvalStatus)
    {
        var response = await _truckService.UpdateApprovalStatusAsync(truckId, approvalStatus);

        // If truck is approved, update its status to Available
        if (approvalStatus == ApprovalStatus.Approved)
        {
            // Only update status if approval was successful
            if (response.IsSuccessful)
            {
                var statusUpdateModel = new UpdateTruckStatusRequestModel
                {
                    TruckStatus = TruckStatus.Available
                };

                await _truckService.UpdateTruckStatus(truckId, statusUpdateModel);
            }
        }

        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetMyTrucks")]
    [Authorize(Roles = "driver")]
    public async Task<ActionResult<ApiResponseModel<List<AllTruckResponseModel>>>> GetMyTrucks(string driverId)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var response = await _truckService.GetTrucksByDriverId(driverId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("UpdateTruckPhotos")]
    [Authorize(Roles = "driver,admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> UpdateTruckPhotos([FromBody] UpdateTruckPhotosRequestModel model)
    {
        // Security check for drivers to ensure they can only update their own truck
        if (User.IsInRole("driver"))
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Get the driver's truck ID
            var truckResponse = await _truckService.GetTrucksByDriverId(userId);
            if (!truckResponse.IsSuccessful || truckResponse.Data == null || !truckResponse.Data.Any())
            {
                return new ApiResponseModel<bool>
                {
                    IsSuccessful = false,
                    Message = "You don't have a truck assigned to update",
                    StatusCode = 403
                };
            }

            // Check if they're trying to update a truck that belongs to them
            var driverTruckId = truckResponse.Data.First().Id;
            if (driverTruckId != model.TruckId)
            {
                return new ApiResponseModel<bool>
                {
                    IsSuccessful = false,
                    Message = "You can only update your own truck",
                    StatusCode = 403
                };
            }
        }

        var response = await _truckService.UpdateTruckPhotos(model);
        return StatusCode(response.StatusCode, response);
    }
}