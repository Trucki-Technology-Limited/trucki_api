using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountDeletionController : ControllerBase
    {
        private readonly IAccountDeletionService _accountDeletionService;

        public AccountDeletionController(IAccountDeletionService accountDeletionService)
        {
            _accountDeletionService = accountDeletionService;
        }

        [HttpPost("request")]
        [Authorize(Roles = "driver,cargo owner")]
        public async Task<ActionResult<ApiResponseModel<bool>>> RequestAccountDeletion(
            [FromBody] AccountDeletionRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseModel<bool>
                {
                    IsSuccessful = false,
                    Message = "Invalid input data",
                    StatusCode = 400
                });
            }

            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponseModel<bool>
                {
                    IsSuccessful = false,
                    Message = "Unauthorized access",
                    StatusCode = 401
                });
            }

            var response = await _accountDeletionService.RequestAccountDeletion(userId, model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("status")]
        [Authorize(Roles = "driver,cargo owner")]
        public async Task<ActionResult<ApiResponseModel<AccountDeletionResponseModel>>> GetAccountDeletionStatus()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponseModel<AccountDeletionResponseModel>
                {
                    IsSuccessful = false,
                    Message = "Unauthorized access",
                    StatusCode = 401
                });
            }

            var response = await _accountDeletionService.GetAccountDeletionRequest(userId);
            return StatusCode(response.StatusCode, response);
        }
    }
}