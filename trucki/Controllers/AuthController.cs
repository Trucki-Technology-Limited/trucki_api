using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    [HttpPost("Login")]
    public async Task<ActionResult<ApiResponseModel<LoginResponseModel>>> LoginAsync([FromBody] LoginRequestModel request)
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
    [HttpPost("UpdateUserPassword")]
    [Authorize]
    public async Task<IActionResult> UpdateUserPassword([FromBody] UpdateUsersPasswordRequestModel requestModel)
    {

        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        var result = await _authService.UpdateUserPassword(userId, requestModel);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("ForgotPassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestModel request)
    {
        string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        if (!(Regex.IsMatch(request.Email, emailPattern)))
            return StatusCode(400, "Invalid email address format");

        var result = await _authService.ForgotPasswordAsync(request.Email);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("VerifyResetCode")]
    public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeRequestModel request)
    {
        var result = await _authService.VerifyResetCodeAsync(request.Email, request.ResetCode);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestModel requestModel)
    {
        var result = await _authService.ResetPasswordAsync(requestModel.Email, requestModel.ResetCode, requestModel.NewPassword);
        return StatusCode(result.StatusCode, result);
    }
    [HttpPost("RegisterDeviceToken")]
    [Authorize]
    public async Task<IActionResult> RegisterDeviceToken([FromBody] RegisterDeviceTokenDto model)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _authService.RegisterDeviceTokenAsync(userId, model.Token, model.DeviceType);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("ConfirmEmail")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequestModel request)
    {
        if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.Token))
        {
            return BadRequest("User ID and token are required");
        }

        var result = await _authService.ConfirmEmailAsync(request.UserId, request.Token);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("ResendEmailConfirmation")]
    public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationRequestModel request)
    {
        string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        if (!(Regex.IsMatch(request.Email, emailPattern)))
            return StatusCode(400, "Invalid email address format");

        var result = await _authService.ResendEmailConfirmationAsync(request.Email);
        return StatusCode(result.StatusCode, result);
    }
}
