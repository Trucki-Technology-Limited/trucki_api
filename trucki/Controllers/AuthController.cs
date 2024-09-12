using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;

namespace trucki.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController: ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    [HttpPost("Login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequestModel request)
    {
        string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        if (!(Regex.IsMatch(request.email, emailPattern)))
            return StatusCode(400, "Invalid email address format");

        var result = await _authService.Login(request);
        return StatusCode(result.StatusCode, result);
    }
    [HttpGet("UserProfile")]
    [Authorize]
    public async Task<IActionResult> GetUserAsync()
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        var result = await _authService.GetUserById(userId);
        return StatusCode(result.StatusCode, result);
    }  
    [HttpPost("ResetUserPassword")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ResetUserPassword([FromBody] ResetUsersPasswordRequestModel requestModel)
    {
        var result = await _authService.ChangePasswordAsync(requestModel.userId, requestModel.password);
        return StatusCode(result.StatusCode, result);
    }
}