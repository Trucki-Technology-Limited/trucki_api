using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers;
[Route("api/[controller]")]
[ApiController]
public class TruckOwnerController: ControllerBase
{
    private readonly ITruckOwnerService _truckOwnerService;

    public TruckOwnerController(ITruckOwnerService truckOwnerService)
    {
        _truckOwnerService = truckOwnerService;
    }
      
    [HttpGet("GetAllTruckOwners")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<List<AllTruckOwnerResponseModel>>>> GetAllTruckOwners()
    {
        var response = await _truckOwnerService.GetAllTruckOwners();
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("DeleteTruckOwner")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> DeleteTruckOwner([FromQuery] string ownerId)
    {
        var response = await _truckOwnerService.DeleteTruckOwner(ownerId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("EditTruckOwner")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> EditTruckOwner(
        [FromBody] EditTruckOwnerRequestBody model)
    {
        var response = await _truckOwnerService.EditTruckOwner(model);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("GetTruckOwnerById")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<TruckOwnerResponseModel>>> GetTruckOwnerById(
        [FromQuery] string ownerId)
    {
        var response = await _truckOwnerService.GetTruckOwnerById(ownerId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("CreateNewTruckOwner")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> CreateNewTruckOwner(
        [FromBody] AddTruckOwnerRequestBody model)
    {
        var response = await _truckOwnerService.CreateNewTruckOwner(model);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("SearchTruckOwners")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<IEnumerable<AllTruckOwnerResponseModel>>>> SearchTruckOwners(string searchWords)
    {
        var response = await _truckOwnerService.SearchTruckOwners(searchWords);
        return StatusCode(response.StatusCode, response);
    }
     [HttpPost("CreateTransporter")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> CreateTransporter(
        [FromBody] AddTruckOwnerRequestBody model)
    {
        var response = await _truckOwnerService.CreateNewTruckOwner(model);
        return StatusCode(response.StatusCode, response);
    }
    [HttpPost("AddNewTransporter")]
    public async Task<ActionResult<ApiResponseModel<bool>>> AddNewTransporter(
        [FromBody] AddTransporterRequestBody model)
    {
        var response = await _truckOwnerService.AddNewTransporter(model);
        return StatusCode(response.StatusCode, response);
    }
     [HttpGet("GetTransporterProfileById")]
    [Authorize(Roles = "transporter")]
    public async Task<ActionResult<ApiResponseModel<TruckOwnerResponseModel>>> GetTransporterProfileById()
    {
        
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        var response = await _truckOwnerService.GetTransporterProfileById(userId);
        return StatusCode(response.StatusCode, response);
    }
     [HttpPost("ApproveTruckOwner")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> ApproveTruckOwner([FromQuery] string ownerId)
    {
        var response = await _truckOwnerService.ApproveTruckOwner(ownerId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("NotApproveTruckOwner")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> NotApproveTruckOwner([FromQuery] string ownerId)
    {
        var response = await _truckOwnerService.NotApproveTruckOwner(ownerId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("BlockTruckOwner")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> BlockTruckOwner([FromQuery] string ownerId)
    {
        var response = await _truckOwnerService.BlockTruckOwner(ownerId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("UnblockTruckOwner")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> UnblockTruckOwner([FromQuery] string ownerId)
    {
        var response = await _truckOwnerService.UnblockTruckOwner(ownerId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpPost("UploadIdCardAndProfilePicture")]
[Authorize(Roles = "transporter")]
public async Task<ActionResult<ApiResponseModel<bool>>> UploadIdCardAndProfilePicture(
    [FromBody] UploadIdCardAndProfilePictureRequestBody model)
{
    var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
    {
        return Unauthorized();
    }

    var response = await _truckOwnerService.UploadIdCardAndProfilePicture(model.Id, model.IdCardUrl, model.ProfilePictureUrl);
    return StatusCode(response.StatusCode, response);
}

[HttpPost("UpdateBankDetails")]
[Authorize(Roles = "transporter")]
public async Task<ActionResult<ApiResponseModel<bool>>> UpdateBankDetails(
    [FromBody] UpdateBankDetailsRequestBody model)
{
    var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
    {
        return Unauthorized();
    }

    var response = await _truckOwnerService.UpdateBankDetails(model);
    return StatusCode(response.StatusCode, response);
}
}