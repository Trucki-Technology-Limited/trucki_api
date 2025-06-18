using Microsoft.EntityFrameworkCore;
using Stripe;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services;

public class StripeConnectService : IStripeConnectService
{
    private readonly IConfiguration _configuration;
    private readonly TruckiDBContext _dbContext;
    private readonly ILogger<StripeConnectService> _logger;
    private readonly AccountService _accountService;
    private readonly AccountLinkService _accountLinkService;
    private readonly TransferService _transferService;

    public StripeConnectService(
        IConfiguration configuration,
        TruckiDBContext dbContext,
        ILogger<StripeConnectService> logger)
    {
        _configuration = configuration;
        _dbContext = dbContext;
        _logger = logger;

        // Initialize Stripe with your secret key
        StripeConfiguration.ApiKey = _configuration.GetSection("Stripe:SecretKey").Value;

        _accountService = new AccountService();
        _accountLinkService = new AccountLinkService();
        _transferService = new TransferService();
    }

    public async Task<ApiResponseModel<StripeConnectAccountResponseModel>> CreateConnectAccountAsync(CreateStripeConnectAccountRequestModel request)
    {
        try
        {
            var driver = await _dbContext.Drivers
                .FirstOrDefaultAsync(d => d.Id == request.DriverId);

            if (driver == null)
            {
                return ApiResponseModel<StripeConnectAccountResponseModel>.Fail("Driver not found", 404);
            }

            // Check if driver already has a Stripe account
            if (!string.IsNullOrEmpty(driver.StripeConnectAccountId))
            {
                // Return existing account link
                var existingAccountLink = await CreateAccountLinkAsync(driver.StripeConnectAccountId);
                return ApiResponseModel<StripeConnectAccountResponseModel>.Success(
                    "Existing Stripe account found",
                    new StripeConnectAccountResponseModel
                    {
                        DriverId = driver.Id,
                        StripeAccountId = driver.StripeConnectAccountId,
                        OnboardingUrl = existingAccountLink.Url,
                        ReturnUrl = GetReturnUrl(),
                        RefreshUrl = GetRefreshUrl(),
                        Status = driver.StripeAccountStatus,
                        CanReceivePayouts = driver.CanReceivePayouts,
                        CreatedAt = driver.StripeAccountCreatedAt ?? DateTime.UtcNow
                    },
                    200);
            }

            // Create new Connect account
            var accountOptions = new AccountCreateOptions
            {
                Type = "custom", // Using Custom accounts for full control
                Country = request.Country ?? "US",
                Email = request.Email ?? driver.EmailAddress,
                Capabilities = new AccountCapabilitiesOptions
                {
                    Transfers = new AccountCapabilitiesTransfersOptions { Requested = true }
                },
                BusinessType = request.BusinessType ?? "individual",
                BusinessProfile = new AccountBusinessProfileOptions
                {
                    Url = "https://trucki.co/driver-info", // Placeholder per-driver profile
                    Mcc = "4789" // Transportation Services
                },
                Metadata = new Dictionary<string, string>
                {
                    {"driver_id", driver.Id},
                    {"driver_name", driver.Name},
                    {"platform", "trucki"}
                }
            };

            var account = await _accountService.CreateAsync(accountOptions);

            // Update driver with Stripe account info
            driver.StripeConnectAccountId = account.Id;
            driver.StripeAccountStatus = StripeAccountStatus.Pending;
            driver.StripeAccountCreatedAt = DateTime.UtcNow;
            driver.CanReceivePayouts = false;

            await _dbContext.SaveChangesAsync();

            // Create account link for onboarding
            var accountLink = await CreateAccountLinkAsync(account.Id);

            _logger.LogInformation("Created Stripe Connect account {AccountId} for driver {DriverId}",
                account.Id, driver.Id);

            return ApiResponseModel<StripeConnectAccountResponseModel>.Success(
                "Stripe Connect account created successfully",
                new StripeConnectAccountResponseModel
                {
                    DriverId = driver.Id,
                    StripeAccountId = account.Id,
                    OnboardingUrl = accountLink.Url,
                    ReturnUrl = GetReturnUrl(),
                    RefreshUrl = GetRefreshUrl(),
                    Status = StripeAccountStatus.Pending,
                    CanReceivePayouts = false,
                    CreatedAt = DateTime.UtcNow
                },
                201);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating Connect account for driver {DriverId}: {Error}",
                request.DriverId, ex.Message);
            return ApiResponseModel<StripeConnectAccountResponseModel>.Fail(
                $"Stripe error: {ex.Message}", 400);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Connect account for driver {DriverId}", request.DriverId);
            return ApiResponseModel<StripeConnectAccountResponseModel>.Fail(
                "An error occurred while creating the Stripe account", 500);
        }
    }

    public async Task<ApiResponseModel<bool>> UpdateAccountStatusAsync(UpdateStripeAccountStatusRequestModel request)
    {
        try
        {
            var driver = await _dbContext.Drivers
                .FirstOrDefaultAsync(d => d.Id == request.DriverId);

            if (driver == null)
            {
                return ApiResponseModel<bool>.Fail("Driver not found", 404);
            }

            // Verify the account in Stripe
            var account = await _accountService.GetAsync(request.StripeAccountId);

            // Update driver status based on Stripe account status
            driver.StripeAccountStatus = MapStripeStatus(account);
            driver.CanReceivePayouts = account.ChargesEnabled && account.PayoutsEnabled;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated Stripe account status for driver {DriverId}: {Status}, CanReceivePayouts: {CanReceivePayouts}",
                driver.Id, driver.StripeAccountStatus, driver.CanReceivePayouts);

            return ApiResponseModel<bool>.Success("Account status updated successfully", true, 200);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error updating account status for driver {DriverId}: {Error}",
                request.DriverId, ex.Message);
            return ApiResponseModel<bool>.Fail($"Stripe error: {ex.Message}", 400);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating account status for driver {DriverId}", request.DriverId);
            return ApiResponseModel<bool>.Fail("An error occurred while updating account status", 500);
        }
    }

    public async Task<ApiResponseModel<string>> RefreshAccountLinkAsync(string driverId)
    {
        try
        {
            var driver = await _dbContext.Drivers
                .FirstOrDefaultAsync(d => d.Id == driverId);

            if (driver == null || string.IsNullOrEmpty(driver.StripeConnectAccountId))
            {
                return ApiResponseModel<string>.Fail("Driver or Stripe account not found", 404);
            }

            var accountLink = await CreateAccountLinkAsync(driver.StripeConnectAccountId);

            return ApiResponseModel<string>.Success("Account link refreshed", accountLink.Url, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing account link for driver {DriverId}", driverId);
            return ApiResponseModel<string>.Fail("Error refreshing account link", 500);
        }
    }

    public async Task<ApiResponseModel<bool>> TransferToDriverAsync(string driverId, decimal amount, string description, string? orderId = null)
    {
        try
        {
            var driver = await _dbContext.Drivers
                .FirstOrDefaultAsync(d => d.Id == driverId);

            if (driver == null || string.IsNullOrEmpty(driver.StripeConnectAccountId))
            {
                return ApiResponseModel<bool>.Fail("Driver or Stripe account not found", 404);
            }

            if (!driver.CanReceivePayouts)
            {
                return ApiResponseModel<bool>.Fail("Driver's Stripe account cannot receive payouts yet", 400);
            }

            var transferOptions = new TransferCreateOptions
            {
                Amount = (long)(amount * 100), // Convert to cents
                Currency = "usd",
                Destination = driver.StripeConnectAccountId,
                Description = description,
                Metadata = new Dictionary<string, string>
                {
                    {"driver_id", driverId},
                    {"driver_name", driver.Name}
                }
            };

            if (!string.IsNullOrEmpty(orderId))
            {
                transferOptions.Metadata.Add("order_id", orderId);
            }

            var transfer = await _transferService.CreateAsync(transferOptions);

            _logger.LogInformation("Created transfer {TransferId} for ${Amount} to driver {DriverId}",
                transfer.Id, amount, driverId);

            return ApiResponseModel<bool>.Success("Transfer created successfully", true, 200);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating transfer for driver {DriverId}: {Error}",
                driverId, ex.Message);
            return ApiResponseModel<bool>.Fail($"Transfer failed: {ex.Message}", 400);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transfer for driver {DriverId}", driverId);
            return ApiResponseModel<bool>.Fail("An error occurred during transfer", 500);
        }
    }

    public async Task<ApiResponseModel<bool>> VerifyAccountCanReceivePayoutsAsync(string stripeAccountId)
    {
        try
        {
            var account = await _accountService.GetAsync(stripeAccountId);
            var canReceivePayouts = account.ChargesEnabled && account.PayoutsEnabled;

            return ApiResponseModel<bool>.Success("Account verification completed", canReceivePayouts, 200);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error verifying account {AccountId}: {Error}", stripeAccountId, ex.Message);
            return ApiResponseModel<bool>.Fail($"Verification failed: {ex.Message}", 400);
        }
    }

    public async Task<ApiResponseModel<StripeConnectAccountResponseModel>> GetDriverStripeInfoAsync(string driverId)
    {
        try
        {
            var driver = await _dbContext.Drivers
                .FirstOrDefaultAsync(d => d.Id == driverId);

            if (driver == null)
            {
                return ApiResponseModel<StripeConnectAccountResponseModel>.Fail("Driver not found", 404);
            }

            // If no Stripe account exists, create one
            if (string.IsNullOrEmpty(driver.StripeConnectAccountId))
            {
                var createRequest = new CreateStripeConnectAccountRequestModel
                {
                    DriverId = driverId,
                    Email = driver.EmailAddress,
                    Country = driver.Country ?? "US",
                    BusinessType = "individual"
                };

                return await CreateConnectAccountAsync(createRequest);
            }

            // Get fresh account link for existing account
            var accountLink = await CreateAccountLinkAsync(driver.StripeConnectAccountId);

            return ApiResponseModel<StripeConnectAccountResponseModel>.Success(
                "Driver Stripe account information retrieved",
                new StripeConnectAccountResponseModel
                {
                    DriverId = driver.Id,
                    StripeAccountId = driver.StripeConnectAccountId,
                    OnboardingUrl = accountLink.Url,
                    ReturnUrl = GetReturnUrl(),
                    RefreshUrl = GetRefreshUrl(),
                    Status = driver.StripeAccountStatus,
                    CanReceivePayouts = driver.CanReceivePayouts,
                    CreatedAt = driver.StripeAccountCreatedAt ?? DateTime.UtcNow
                },
                200);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error getting driver info for {DriverId}: {Error}", driverId, ex.Message);
            return ApiResponseModel<StripeConnectAccountResponseModel>.Fail($"Stripe error: {ex.Message}", 400);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting driver Stripe info for {DriverId}", driverId);
            return ApiResponseModel<StripeConnectAccountResponseModel>.Fail(
                "An error occurred while retrieving driver information", 500);
        }
    }

    private async Task<AccountLink> CreateAccountLinkAsync(string accountId)
    {
        var accountLinkOptions = new AccountLinkCreateOptions
        {
            Account = accountId,
            RefreshUrl = GetRefreshUrl(),
            ReturnUrl = GetReturnUrl(),
            Type = "account_onboarding"
        };

        return await _accountLinkService.CreateAsync(accountLinkOptions);
    }

    private string GetReturnUrl()
    {
        var baseUrl = _configuration.GetValue<string>("BaseUrl") ?? "https://api.trucki.co";
        return $"{baseUrl}/api/driverstripe/verify-onboarding";
    }

    private string GetRefreshUrl()
    {
        var baseUrl = _configuration.GetValue<string>("BaseUrl") ?? "https://api.trucki.co";
        return $"{baseUrl}/api/driverstripe/refresh-account-link";
    }

    private static StripeAccountStatus MapStripeStatus(Account account)
    {
        if (account.ChargesEnabled && account.PayoutsEnabled)
            return StripeAccountStatus.Active;

        if (account.Requirements?.CurrentlyDue?.Any() == true ||
            account.Requirements?.EventuallyDue?.Any() == true)
            return StripeAccountStatus.Restricted;

        return StripeAccountStatus.Pending;
    }
}