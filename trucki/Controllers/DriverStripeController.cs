using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Ensure proper authentication
    public class DriverStripeController : ControllerBase
    {
        private readonly IStripeConnectService _stripeConnectService;
        private readonly IDriverPayoutService _payoutService;
        private readonly ILogger<DriverStripeController> _logger;

        public DriverStripeController(
            IStripeConnectService stripeConnectService,
            IDriverPayoutService payoutService,
            ILogger<DriverStripeController> logger)
        {
            _stripeConnectService = stripeConnectService;
            _payoutService = payoutService;
            _logger = logger;
        }

        /// <summary>
        /// Create a Stripe Connect account for a driver
        /// </summary>
        [HttpPost("create-account")]
        public async Task<IActionResult> CreateStripeAccount([FromBody] CreateStripeConnectAccountRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseModel<object>
                {
                    IsSuccessful = false,
                    Message = "Invalid request data",
                    StatusCode = 400,
                    Data = null
                });
            }

            var result = await _stripeConnectService.CreateConnectAccountAsync(request);
            
            return result.StatusCode switch
            {
                200 or 201 => Ok(new ApiResponseModel<StripeConnectAccountResponseModel>
                {
                    IsSuccessful = true,
                    Message = result.Message,
                    StatusCode = result.StatusCode,
                    Data = result.Data
                }),
                404 => NotFound(new ApiResponseModel<object>
                {
                    IsSuccessful = false,
                    Message = result.Message,
                    StatusCode = 404,
                    Data = null
                }),
                400 => BadRequest(new ApiResponseModel<object>
                {
                    IsSuccessful = false,
                    Message = result.Message,
                    StatusCode = 400,
                    Data = null
                }),
                _ => StatusCode(500, new ApiResponseModel<object>
                {
                    IsSuccessful = false,
                    Message = result.Message,
                    StatusCode = 500,
                    Data = null
                })
            };
        }

        /// <summary>
        /// Update a driver's Stripe account status (typically called from webhooks)
        /// </summary>
        [HttpPut("update-account-status")]
        public async Task<IActionResult> UpdateAccountStatus([FromBody] UpdateStripeAccountStatusRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseModel<object>
                {
                    IsSuccessful = false,
                    Message = "Invalid request data",
                    StatusCode = 400,
                    Data = null
                });
            }

            var result = await _stripeConnectService.UpdateAccountStatusAsync(request);
            
            return result.StatusCode switch
            {
                200 => Ok(new ApiResponseModel<bool>
                {
                    IsSuccessful = true,
                    Message = result.Message,
                    StatusCode = 200,
                    Data = result.Data
                }),
                404 => NotFound(new ApiResponseModel<object>
                {
                    IsSuccessful = false,
                    Message = result.Message,
                    StatusCode = 404,
                    Data = null
                }),
                400 => BadRequest(new ApiResponseModel<object>
                {
                    IsSuccessful = false,
                    Message = result.Message,
                    StatusCode = 400,
                    Data = null
                }),
                _ => StatusCode(500, new ApiResponseModel<object>
                {
                    IsSuccessful = false,
                    Message = result.Message,
                    StatusCode = 500,
                    Data = null
                })
            };
        }

        /// <summary>
        /// Refresh the Stripe Connect account onboarding link for a driver
        /// </summary>
        [HttpPost("refresh-account-link/{driverId}")]
        public async Task<IActionResult> RefreshAccountLink(string driverId)
        {
            if (string.IsNullOrEmpty(driverId))
            {
                return BadRequest(new ApiResponseModel<object>
                {
                    IsSuccessful = false,
                    Message = "Driver ID is required",
                    StatusCode = 400,
                    Data = null
                });
            }

            var result = await _stripeConnectService.RefreshAccountLinkAsync(driverId);
            
            return result.StatusCode switch
            {
                200 => Ok(new ApiResponseModel<string>
                {
                    IsSuccessful = true,
                    Message = result.Message,
                    StatusCode = 200,
                    Data = result.Data
                }),
                404 => NotFound(new ApiResponseModel<object>
                {
                    IsSuccessful = false,
                    Message = result.Message,
                    StatusCode = 404,
                    Data = null
                }),
                _ => StatusCode(500, new ApiResponseModel<object>
                {
                    IsSuccessful = false,
                    Message = result.Message,
                    StatusCode = 500,
                    Data = null
                })
            };
        }

        /// <summary>
        /// Get driver's payout history
        /// </summary>
        [HttpGet("payouts/{driverId}")]
        public async Task<IActionResult> GetDriverPayouts(string driverId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrEmpty(driverId))
            {
                return BadRequest(new ApiResponseModel<object>
                {
                    IsSuccessful = false,
                    Message = "Driver ID is required",
                    StatusCode = 400,
                    Data = null
                });
            }

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _payoutService.GetDriverPayoutHistoryAsync(driverId, page, pageSize);
            
            return result.StatusCode switch
            {
                200 => Ok(new ApiResponseModel<List<DriverPayoutResponseModel>>
                {
                    IsSuccessful = true,
                    Message = result.Message,
                    StatusCode = 200,
                    Data = result.Data
                }),
                404 => NotFound(new ApiResponseModel<object>
                {
                    IsSuccessful = false,
                    Message = result.Message,
                    StatusCode = 404,
                    Data = null
                }),
                _ => StatusCode(500, new ApiResponseModel<object>
                {
                    IsSuccessful = false,
                    Message = result.Message,
                    StatusCode = 500,
                    Data = null
                })
            };
        }

        /// <summary>
        /// Get driver's earnings projection for upcoming payout
        /// </summary>
        [HttpGet("earnings-projection/{driverId}")]
        public async Task<IActionResult> GetDriverEarningsProjection(string driverId)
        {
            if (string.IsNullOrEmpty(driverId))
            {
                return BadRequest(new ApiResponseModel<object>
                {
                    IsSuccessful = false,
                    Message = "Driver ID is required",
                    StatusCode = 400,
                    Data = null
                });
            }

            var result = await _payoutService.GetDriverEarningsProjectionAsync(driverId);
            
            return result.StatusCode switch
            {
                200 => Ok(new ApiResponseModel<DriverEarningsProjectionModel>
                {
                    IsSuccessful = true,
                    Message = result.Message,
                    StatusCode = 200,
                    Data = result.Data
                }),
                404 => NotFound(new ApiResponseModel<object>
                {
                    IsSuccessful = false,
                    Message = result.Message,
                    StatusCode = 404,
                    Data = null
                }),
                _ => StatusCode(500, new ApiResponseModel<object>
                {
                    IsSuccessful = false,
                    Message = result.Message,
                    StatusCode = 500,
                    Data = null
                })
            };
        }

        /// <summary>
        /// Check driver's Stripe account status and get onboarding link if needed
        /// Mobile-friendly endpoint that returns URLs instead of redirecting
        /// </summary>
        [HttpGet("account-status/{driverId}")]
        public async Task<IActionResult> GetAccountStatus(string driverId)
        {
            if (string.IsNullOrEmpty(driverId))
            {
                return BadRequest(new ApiResponseModel<object>
                {
                    IsSuccessful = false,
                    Message = "Driver ID is required",
                    StatusCode = 400,
                    Data = null
                });
            }

            try
            {
                // Get driver's current Stripe account info
                var driver = await _stripeConnectService.GetDriverStripeInfoAsync(driverId);
                
                if (!driver.IsSuccessful)
                {
                    return StatusCode(driver.StatusCode, new ApiResponseModel<object>
                    {
                        IsSuccessful = false,
                        Message = driver.Message,
                        StatusCode = driver.StatusCode,
                        Data = null
                    });
                }

                var response = new
                {
                    HasStripeAccount = !string.IsNullOrEmpty(driver.Data.StripeAccountId),
                    AccountStatus = driver.Data.Status.ToString(),
                    CanReceivePayouts = driver.Data.CanReceivePayouts,
                    OnboardingUrl = driver.Data.OnboardingUrl,
                    RequiresOnboarding = driver.Data.Status != StripeAccountStatus.Active,
                    NextSteps = GetNextStepsMessage(driver.Data.Status, driver.Data.CanReceivePayouts)
                };

                return Ok(new ApiResponseModel<object>
                {
                    IsSuccessful = true,
                    Message = "Account status retrieved successfully",
                    StatusCode = 200,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account status for driver {DriverId}", driverId);
                return StatusCode(500, new ApiResponseModel<object>
                {
                    IsSuccessful = false,
                    Message = "Error retrieving account status",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        /// <summary>
        /// Handle Stripe Connect onboarding completion status for mobile
        /// Returns account verification status without redirecting
        /// </summary>
        [HttpGet("verify-onboarding")]
        [AllowAnonymous] // This endpoint might be called from Stripe redirect
        public async Task<IActionResult> VerifyOnboardingStatus([FromQuery] string account)
        {
            if (string.IsNullOrEmpty(account))
            {
                return BadRequest(new ApiResponseModel<object>
                {
                    IsSuccessful = false,
                    Message = "Account ID is required",
                    StatusCode = 400,
                    Data = null
                });
            }

            try
            {
                // Verify account status
                var verifyResult = await _stripeConnectService.VerifyAccountCanReceivePayoutsAsync(account);
                
                var response = new
                {
                    AccountId = account,
                    CanReceivePayouts = verifyResult.IsSuccessful && verifyResult.Data,
                    Status = verifyResult.IsSuccessful && verifyResult.Data ? "completed" : "pending",
                    Message = verifyResult.IsSuccessful && verifyResult.Data 
                        ? "Your Stripe account is fully set up and ready to receive payouts!"
                        : "Your Stripe account setup is still in progress. You may need to provide additional information.",
                    RequiresAdditionalInfo = !verifyResult.IsSuccessful || !verifyResult.Data
                };

                return Ok(new ApiResponseModel<object>
                {
                    IsSuccessful = true,
                    Message = "Account verification completed",
                    StatusCode = 200,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying onboarding status for account {AccountId}", account);
                return StatusCode(500, new ApiResponseModel<object>
                {
                    IsSuccessful = false,
                    Message = "Error verifying account status",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        private string GetNextStepsMessage(StripeAccountStatus status, bool canReceivePayouts)
        {
            return status switch
            {
                StripeAccountStatus.NotCreated => "Create your Stripe account to start receiving payouts",
                StripeAccountStatus.Pending => "Complete your Stripe account setup to receive payouts",
                StripeAccountStatus.Restricted => "Please provide additional information to activate your account",
                StripeAccountStatus.Active when canReceivePayouts => "Your account is ready to receive payouts",
                StripeAccountStatus.Active => "Account active but additional verification needed for payouts",
                StripeAccountStatus.Rejected => "Account setup was rejected. Please contact support",
                _ => "Please complete your account setup"
            };
        }
    }
}