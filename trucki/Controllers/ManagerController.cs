using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ManagerController : ControllerBase
{
    private readonly IManagerService _managerService;

    public ManagerController(IManagerService managerService)
    {
        _managerService = managerService;
    }

    [HttpPost("AddManager")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> AddManager([FromBody] AddManagerRequestModel model)
    {
        var response = await _managerService.AddManager(model);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetAllManager")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<AllManagerResponseModel>>> GetAllManager()
    {
        var business = await _managerService.GetAllManager();
        return StatusCode(business.StatusCode, business);
    }

    [HttpGet("GetManager")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<AllManagerResponseModel>>> GetManagerById(string id)
    {
        var manager = await _managerService.GetManagerById(id);
        return StatusCode(manager.StatusCode, manager);
    }

    [HttpPost("EditManager")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> EditManager([FromBody] EditManagerRequestModel model)
    {
        var response = await _managerService.EditManager(model);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("DeleteManager")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> DeleteManager([FromQuery] string managerId)
    {
        var response = await _managerService.DeactivateManager(managerId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("SearchManagers")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<IEnumerable<AllManagerResponseModel>>>> SearchManagers(string searchWords)
    {
        var response = await _managerService.SearchManagers(searchWords);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetManagerDashboardData")]
    [Authorize(Roles = "manager,finance,chiefmanager,field officer")]
    public async Task<ActionResult<ApiResponseModel<ManagerDashboardData>>> GetManagerDashboardSummary()
    {
        var roles = User.Claims
                      .Where(c => c.Type == ClaimTypes.Role)
                      .Select(c => c.Value)
                      .ToList();
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        var response = await _managerService.GetManagerDashboardData(roles, userId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetTransactionsByManager")]
    [Authorize(Roles = "manager,finance,chiefmanager")]
    public async Task<ActionResult<ApiResponseModel<List<TransactionResponseModel>>>> GetTransactionsByManager()
    {
        var roles = User.Claims
                      .Where(c => c.Type == ClaimTypes.Role)
                      .Select(c => c.Value)
                      .ToList();
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        var response = await _managerService.GetTransactionsByManager(roles, userId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetTransactionSummaryResponseModel")]
    [Authorize(Roles = "manager,finance,chiefmanager")]
    public async Task<ActionResult<ApiResponseModel<TransactionSummaryResponseModel>>> GetTransactionSummaryResponseModel()
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        var response = await _managerService.GetTransactionSummaryResponseModel(userId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetTransactionsByFinancialManager")]
    [Authorize(Roles = "finance")]
    public async Task<ActionResult<ApiResponseModel<List<TransactionResponseModel>>>> GetTransactionsByFinancialManager()
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        var response = await _managerService.GetTransactionsByFinancialManager(userId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetFinancialTransactionSummaryResponseModel")]
    [Authorize(Roles = "finance")]
    public async Task<ActionResult<ApiResponseModel<TransactionSummaryResponseModel>>> GetFinancialTransactionSummaryResponseModel()
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        var response = await _managerService.GetFinancialTransactionSummaryResponseModel(userId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("GetManagerGtvDashBoardSummary")]
    [Authorize(Roles = "manager,finance,chiefmanager,field officer")]
    public async Task<ActionResult<ApiResponseModel<GtvDashboardSummary>>> GetManagerGtvDashBoardSummary(DateTime startDate, DateTime endDate)
    {
        var roles = User.Claims
                       .Where(c => c.Type == ClaimTypes.Role)
                       .Select(c => c.Value)
                       .ToList();
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        var response = await _managerService.GetManagerGtvDashBoardSummary(startDate, endDate, roles, userId);
        return StatusCode(response.StatusCode, response);
    }
    [HttpPut("EditAssignedBusinesses")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> EditAssignedBusinesses([FromBody] EditAssignedBusinessesRequestModel model)
    {
        var response = await _managerService.EditAssignedBusinesses(model);

        if (response.IsSuccessful)
        {
            return Ok(response);
        }

        return StatusCode(response.StatusCode, response);
    }

}