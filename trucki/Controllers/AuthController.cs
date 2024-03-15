using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using trucki.DTOs;
using trucki.Entities;
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
    [ProducesResponseType(typeof(ApiResponseModel<CreatTruckiUserResponseDto>), (int)HttpStatusCode.Created)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequestModel request)
    {
        var result = await _authService.Login(request);
        return StatusCode(result.StatusCode, result);
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
    public async Task<IActionResult> RefreshToken(RefreshTokenResponseDto refreshToken)
    {

        var result = await _authService.RefreshToken(refreshToken.RefreshToken);
        return StatusCode(result.StatusCode, result);
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
}