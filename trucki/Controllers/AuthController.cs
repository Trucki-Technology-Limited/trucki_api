using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using trucki.DTOs;
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
        string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        if (!(Regex.IsMatch(request.email, emailPattern)))
            return StatusCode(400, "Invalid email address format");

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
    public async Task<IActionResult> RefreshToken(string refreshToken)
    {

        var result = await _authService.RefreshToken(refreshToken);
        return StatusCode(result.StatusCode, result);
    }
}