using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FieldOfficerController : ControllerBase
{
    private readonly IFieldOfficerService _fieldOfficerService;

    public FieldOfficerController(IFieldOfficerService fieldOfficerService)
    {
        _fieldOfficerService = fieldOfficerService;
    }

    [HttpPost("CreateNewOfficer")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<string>>> CreateNewOfficer([FromBody] AddOfficerRequestModel model)
    {
        var response = await _fieldOfficerService.AddOfficer(model);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("GetAllFieldOfficers")]
    [Authorize(Roles = "admin,manager,field officer,chiefmanager")]
    public async Task<ActionResult<ApiResponseModel<PaginatedListDto<AllOfficerResponseModel>>>> GetAllFieldOfficers(int page, int size)
    {
        var response = await _fieldOfficerService.GetAllFieldOfficers(page, size);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("EditOfficer")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> EditManager([FromBody] EditOfficerRequestModel model)
    {
        var response = await _fieldOfficerService.EditOfficer(model);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("GetOfficerById")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<AllOfficerResponseModel>>> GetOfficerById(string officerId)
    {
        var response = await _fieldOfficerService.GetOfficerById(officerId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("DeleteOfficer")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<string>>> DeleteOfficer(string officerId)
    {
        var response = await _fieldOfficerService.DeleteOfficers(officerId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("SearchOfficers")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<IEnumerable<AllOfficerResponseModel>>>> SearchOfficer(string? searchWords)
    {
        var response = await _fieldOfficerService.SearchOfficer(searchWords);
        return StatusCode(response.StatusCode, response);
    }
    [HttpPost("ReassignOfficerCompany")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<string>>> ReassignOfficerCompany(string officerId, string newCompanyId)
    {
        var response = await _fieldOfficerService.ReassignOfficerCompany(officerId, newCompanyId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetAllSafetyOfficers")]
    [Authorize(Roles = "admin,manager,field officer,chiefmanager")]
    public async Task<ActionResult<ApiResponseModel<PaginatedListDto<AllOfficerResponseModel>>>> GetAllSafetyOfficers(int page, int size)
    {
        var response = await _fieldOfficerService.GetAllSafetyOfficers(page, size);
        return StatusCode(response.StatusCode, response);
    }

}