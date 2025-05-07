using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "cargo owner")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly ICargoOwnerService _cargoOwnerService;

        public WalletController(IWalletService walletService, ICargoOwnerService cargoOwnerService)
        {
            _walletService = walletService;
            _cargoOwnerService = cargoOwnerService;
        }

        [HttpGet("balance")]
        public async Task<ActionResult<ApiResponseModel<decimal>>> GetBalance()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Get the cargo owner profile to find the CargoOwnerId
                var profileResult = await _cargoOwnerService.GetCargoOwnerProfile(userId);
                if (!profileResult.IsSuccessful || profileResult.Data == null)
                {
                    return StatusCode(
                        profileResult.StatusCode,
                        ApiResponseModel<decimal>.Fail(profileResult.Message, profileResult.StatusCode));
                }

                var cargoOwnerId = profileResult.Data.Id;
                var balance = await _walletService.GetWalletBalance(cargoOwnerId);

                return Ok(ApiResponseModel<decimal>.Success(
                    "Balance retrieved successfully",
                    balance,
                    200));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseModel<decimal>.Fail(
                    $"Error retrieving wallet balance: {ex.Message}",
                    500));
            }
        }

        [HttpGet("transactions")]
        public async Task<ActionResult<ApiResponseModel<PagedResponse<WalletTransactionResponseModel>>>> GetTransactions(
            [FromQuery] GetWalletTransactionsQueryDto query)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Get the cargo owner profile to find the CargoOwnerId
                var profileResult = await _cargoOwnerService.GetCargoOwnerProfile(userId);
                if (!profileResult.IsSuccessful || profileResult.Data == null)
                {
                    return StatusCode(
                        profileResult.StatusCode,
                        ApiResponseModel<PagedResponse<WalletTransactionResponseModel>>.Fail(
                            profileResult.Message,
                            profileResult.StatusCode));
                }

                var cargoOwnerId = profileResult.Data.Id;
                var result = await _walletService.GetWalletTransactions(cargoOwnerId, query);

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseModel<PagedResponse<WalletTransactionResponseModel>>.Fail(
                    $"Error retrieving wallet transactions: {ex.Message}",
                    500));
            }
        }
    }
}