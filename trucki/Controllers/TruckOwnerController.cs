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
    public async Task<ActionResult<ApiResponseModel<List<TruckOwnerResponseModel>>>> GetAllTruckOwners()
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
        [FromForm] EditTruckOwnerRequestBody model)
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
        [FromForm] AddTruckOwnerRequestBody model)
    {
        var response = await _truckOwnerService.CreateNewTruckOwner(model);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("SearchTruckOwners")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<IEnumerable<TruckOwnerResponseModel>>>> SearchTruckOwners(string searchWords)
    {
        var response = await _truckOwnerService.SearchTruckOwners(searchWords);
        return StatusCode(response.StatusCode, response);
    }

}