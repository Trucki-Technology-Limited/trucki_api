using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services;

/// <summary>
/// Integrated service that handles both wallet-based and Stripe Connect payouts
/// </summary>
public class IntegratedPayoutService : IIntegratedPayoutService
{
    private readonly TruckiDBContext _dbContext;
    private readonly IDriverWalletService _walletService;
    private readonly IDriverPayoutService _stripePayoutService;
    private readonly ILogger<IntegratedPayoutService> _logger;

    public IntegratedPayoutService(
        TruckiDBContext dbContext,
        IDriverWalletService walletService,
        IDriverPayoutService stripePayoutService,
        ILogger<IntegratedPayoutService> logger)
    {
        _dbContext = dbContext;
        _walletService = walletService;
        _stripePayoutService = stripePayoutService;
        _logger = logger;
    }

    /// <summary>
    /// Determines the best payout method for each driver and processes accordingly
    /// </summary>
    public async Task<ApiResponseModel<IntegratedPayoutSummaryModel>> ProcessWeeklyPayoutsAsync(string processedBy)
    {
        try
        {
            _logger.LogInformation("Starting integrated weekly payout processing");

            var summary = new IntegratedPayoutSummaryModel
            {
                ProcessedAt = DateTime.UtcNow
            };

            // Get all active drivers
            var drivers = await _dbContext.Drivers
                .Where(d => d.IsActive)
                .ToListAsync();

            foreach (var driver in drivers)
            {
                try
                {
                    var payoutMethod = await DeterminePayoutMethod(driver);
                    
                    switch (payoutMethod)
                    {
                        case PayoutMethod.StripeConnect:
                            await ProcessStripeConnectPayout(driver, summary, processedBy);
                            break;
                            
                        case PayoutMethod.Wallet:
                            await ProcessWalletPayout(driver, summary, processedBy);
                            break;
                            
                        case PayoutMethod.None:
                            summary.SkippedDrivers++;
                            _logger.LogInformation("Skipping driver {DriverId} - no valid payout method", driver.Id);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    summary.FailedPayouts++;
                    summary.Errors.Add(new PayoutErrorModel
                    {
                        DriverId = driver.Id,
                        DriverName = driver.Name,
                        ErrorMessage = ex.Message
                    });
                    
                    _logger.LogError(ex, "Error processing payout for driver {DriverId}", driver.Id);
                }
            }

            _logger.LogInformation("Integrated payout processing completed. Stripe: {StripeCount}, Wallet: {WalletCount}, Failed: {FailedCount}",
                summary.StripePayouts, summary.WalletPayouts, summary.FailedPayouts);

            return ApiResponseModel<IntegratedPayoutSummaryModel>.Success(
                "Integrated payout processing completed",
                summary,
                200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during integrated payout processing");
            return ApiResponseModel<IntegratedPayoutSummaryModel>.Fail(
                "Error occurred during integrated payout processing", 500);
        }
    }

    private async Task<PayoutMethod> DeterminePayoutMethod(Driver driver)
    {
        // Priority 1: Use Stripe Connect if available and active
        if (!string.IsNullOrEmpty(driver.StripeConnectAccountId) && driver.CanReceivePayouts)
        {
            return PayoutMethod.StripeConnect;
        }

        // Priority 2: Use wallet system if driver has bank account configured
        var hasBankAccount = await _dbContext.Set<DriverBankAccount>()
            .AnyAsync(ba => ba.DriverId == driver.Id && ba.IsVerified);

        if (hasBankAccount)
        {
            return PayoutMethod.Wallet;
        }

        // No valid payout method
        return PayoutMethod.None;
    }

    private async Task ProcessStripeConnectPayout(Driver driver, IntegratedPayoutSummaryModel summary, string processedBy)
    {
        var result = await _stripePayoutService.ProcessDriverPayoutAsync(driver.Id, processedBy, false);
        
        if (result.IsSuccessful && result.Data.Amount > 0)
        {
            summary.StripePayouts++;
            summary.TotalStripeAmount += result.Data.Amount;
            
            _logger.LogInformation("Processed Stripe payout for driver {DriverId}: ${Amount}", 
                driver.Id, result.Data.Amount);
        }
        else if (!result.IsSuccessful)
        {
            summary.FailedPayouts++;
            summary.Errors.Add(new PayoutErrorModel
            {
                DriverId = driver.Id,
                DriverName = driver.Name,
                ErrorMessage = $"Stripe payout failed: {result.Message}"
            });
        }
    }

    private async Task ProcessWalletPayout(Driver driver, IntegratedPayoutSummaryModel summary, string processedBy)
    {
        // For wallet payouts, we need to check if there's a balance to withdraw
        var balanceResult = await _walletService.GetWalletBalanceAsync(driver.Id);
        
        if (!balanceResult.IsSuccessful)
        {
            summary.FailedPayouts++;
            summary.Errors.Add(new PayoutErrorModel
            {
                DriverId = driver.Id,
                DriverName = driver.Name,
                ErrorMessage = $"Could not get wallet balance: {balanceResult.Message}"
            });
            return;
        }

        if (balanceResult.Data.AvailableBalance > 0)
        {
            // Process the wallet withdrawal (your existing logic)
            var withdrawalResult = await _walletService.ProcessWeeklyWithdrawalsAsync(processedBy);
            
            if (withdrawalResult.IsSuccessful)
            {
                summary.WalletPayouts++;
                summary.TotalWalletAmount += balanceResult.Data.AvailableBalance;
                
                _logger.LogInformation("Processed wallet payout for driver {DriverId}: ${Amount}", 
                    driver.Id, balanceResult.Data.AvailableBalance);
            }
            else
            {
                summary.FailedPayouts++;
                summary.Errors.Add(new PayoutErrorModel
                {
                    DriverId = driver.Id,
                    DriverName = driver.Name,
                    ErrorMessage = $"Wallet withdrawal failed: {withdrawalResult.Message}"
                });
            }
        }
    }

    /// <summary>
    /// When an order is completed, credit both systems appropriately
    /// </summary>
    public async Task<ApiResponseModel<bool>> ProcessOrderCompletionAsync(string orderId, decimal driverEarnings)
    {
        try
        {
            var order = await _dbContext.Set<CargoOrders>()
                .Include(o => o.AcceptedBid)
                    .ThenInclude(b => b.Truck)
                        .ThenInclude(t => t.Driver)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order?.AcceptedBid?.Truck?.Driver == null)
            {
                return ApiResponseModel<bool>.Fail("Order or driver not found", 404);
            }

            var driver = order.AcceptedBid.Truck.Driver;

            // Update order with driver earnings
            order.DriverEarnings = driverEarnings;
            await _dbContext.SaveChangesAsync();

            // If driver uses Stripe Connect, earnings are handled during payout processing
            if (!string.IsNullOrEmpty(driver.StripeConnectAccountId) && driver.CanReceivePayouts)
            {
                _logger.LogInformation("Order {OrderId} completed for Stripe Connect driver {DriverId}. Earnings will be processed in next payout.",
                    orderId, driver.Id);
                return ApiResponseModel<bool>.Success("Order completion recorded for Stripe Connect payout", true, 200);
            }

            // If driver uses wallet system, credit the wallet immediately
            var walletResult = await _walletService.CreditDeliveryAmountAsync(
                driver.Id,
                orderId,
                driverEarnings,
                $"Delivery completion for order {orderId}");

            if (walletResult.IsSuccessful)
            {
                _logger.LogInformation("Credited ${Amount} to wallet for driver {DriverId} upon order {OrderId} completion",
                    driverEarnings, driver.Id, orderId);
                return ApiResponseModel<bool>.Success("Order completion processed and wallet credited", true, 200);
            }

            return ApiResponseModel<bool>.Fail($"Failed to credit wallet: {walletResult.Message}", 500);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order completion for order {OrderId}", orderId);
            return ApiResponseModel<bool>.Fail("Error processing order completion", 500);
        }
    }
}
