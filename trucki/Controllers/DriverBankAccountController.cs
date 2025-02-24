using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;

namespace trucki.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DriverBankAccountController : ControllerBase
{
    private readonly IDriverBankAccountService _bankAccountService;

    public DriverBankAccountController(IDriverBankAccountService bankAccountService)
    {
        _bankAccountService = bankAccountService;
    }

    [HttpPost]
    [Authorize(Roles = "driver")]
    public async Task<IActionResult> AddBankAccount([FromBody] AddDriverBankAccountDto request)
    {
        // var driverId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // if (string.IsNullOrEmpty(driverId))
        // {
        //     return Unauthorized();
        // }

        var result = await _bankAccountService.AddBankAccountAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet]
    [Authorize(Roles = "driver")]
    public async Task<IActionResult> GetBankAccounts(string driverId)
    {
        // var driverId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // if (string.IsNullOrEmpty(driverId))
        // {
        //     return Unauthorized();
        // }

        var result = await _bankAccountService.GetDriverBankAccountsAsync(driverId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{accountId}/setDefault")]
    [Authorize(Roles = "driver")]
    public async Task<IActionResult> SetDefaultAccount(string accountId)
    {
        var driverId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(driverId))
        {
            return Unauthorized();
        }

        var result = await _bankAccountService.SetDefaultBankAccountAsync(driverId, accountId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{accountId}")]
    [Authorize(Roles = "driver")]
    public async Task<IActionResult> DeleteBankAccount(string accountId)
    {
        var driverId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(driverId))
        {
            return Unauthorized();
        }

        var result = await _bankAccountService.DeleteBankAccountAsync(driverId, accountId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{accountId}/verify")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> VerifyBankAccount(string accountId)
    {
        var adminId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _bankAccountService.VerifyBankAccountAsync(accountId, adminId);
        return StatusCode(result.StatusCode, result);
    }
}