using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using trucki.DTOs;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly UserManager<User> _userManager;
    public AuthController(IAuthService authService, ITokenService tokenService, UserManager<User> userManager)
    {
        _authService = authService;
        _tokenService = tokenService;
        _userManager = userManager;
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    [ProducesResponseType(typeof(ApiResponseModel<CreatTruckiUserResponseDto>), (int)HttpStatusCode.Created)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequestModel request)
    {
        var userLogin = await _authService.Login(request);
        if (userLogin.IsSuccessful && userLogin.Data != null)
        {
            var token = _tokenService.GenerateToken(ref userLogin);
            var refreshToken = _tokenService.GenerateRefreshToken(ref userLogin);

            var loggedInUser = await _userManager.FindByEmailAsync(userLogin.Data.EmailAddress);
            userLogin.Data.Token = token;
            userLogin.Data.RefreshToken = refreshToken;
            userLogin.Data.TokenGenerationTime = DateTime.UtcNow.ToString();
            userLogin.Data.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(loggedInUser);

            return StatusCode(userLogin.StatusCode, userLogin);
        }

        return StatusCode(userLogin.StatusCode, userLogin);
    }

    [AllowAnonymous]
    [HttpPost("Register")]
    [ProducesResponseType(typeof(ApiResponseModel<CreatTruckiUserResponseDto>), (int)HttpStatusCode.Created)]
    public async Task<IActionResult> RegisterTruckiAsync([FromBody] CreatTruckiUserDto request)
    {
        var result = await _authService.RegisterTruckiAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    [Route("Refresh-token")]
    [ProducesResponseType(typeof(ApiResponseModel<CreatTruckiUserResponseDto>), (int)HttpStatusCode.Created)]
    public async Task<IActionResult> RefreshToken(RefreshTokenDto refreshToken)
    {
        var refreshtokenresponse = await _authService.GenerateRefreshToken(refreshToken);

        if (refreshtokenresponse.IsSuccessful && refreshtokenresponse != null)
        {

            return StatusCode(refreshtokenresponse.StatusCode, refreshtokenresponse);
        }

        return StatusCode(refreshtokenresponse.StatusCode, refreshtokenresponse);
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("ConfirmEmail")]
    [ProducesResponseType(typeof(ApiResponseModel<CreatTruckiUserResponseDto>), (int)HttpStatusCode.Created)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string email)
    {

        var result = await _authService.VerifyUser(email);
        return StatusCode(result.StatusCode, result);
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("ForgotPassword")]
    [ProducesResponseType(typeof(ApiResponseModel<string>), (int)HttpStatusCode.Created)]
    public async Task<IActionResult> ForgotPassword([FromQuery] string email)
    {

        var result = await _authService.ForgotPassword(email);
        return StatusCode(result.StatusCode, result);
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("ChangePassword")]
    [ProducesResponseType(typeof(ApiResponseModel<string>), (int)HttpStatusCode.Created)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {

        var result = await _authService.ChangePassword(request);
        return StatusCode(result.StatusCode, result);
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("ResetPassword")]
    [ProducesResponseType(typeof(ApiResponseModel<string>), (int)HttpStatusCode.Created)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {

        var result = await _authService.ResetPassword(request);
        return StatusCode(result.StatusCode, result);
    }
}