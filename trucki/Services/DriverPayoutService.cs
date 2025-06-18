using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.ResponseModels;

namespace trucki.Services;

public class DriverPayoutService : IDriverPayoutService
{
    private readonly TruckiDBContext _dbContext;
    private readonly IStripeConnectService _stripeConnectService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<DriverPayoutService> _logger;

    public DriverPayoutService(
        TruckiDBContext dbContext,
        IStripeConnectService stripeConnectService,
        INotificationService notificationService,
        ILogger<DriverPayoutService> logger)
    {
        _dbContext = dbContext;
        _stripeConnectService = stripeConnectService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<ApiResponseModel<PayoutSummaryResponseModel>> ProcessWeeklyPayoutsAsync(string processedBy)
    {
        try
        {
            _logger.LogInformation("Starting weekly payout processing initiated by {ProcessedBy}", processedBy);

            var summary = new PayoutSummaryResponseModel
            {
                ProcessedAt = DateTime.UtcNow
            };

            // Get all drivers with Stripe accounts that can receive payouts
            var eligibleDrivers = await _dbContext.Drivers
                .Where(d => d.IsActive &&
                           !string.IsNullOrEmpty(d.StripeConnectAccountId) &&
                           d.CanReceivePayouts)
                .ToListAsync();

            summary.TotalDrivers = await _dbContext.Drivers.CountAsync(d => d.IsActive);
            summary.EligibleDrivers = eligibleDrivers.Count;

            foreach (var driver in eligibleDrivers)
            {
                try
                {
                    var payoutResult = await ProcessDriverPayoutAsync(driver.Id, processedBy, false);

                    if (payoutResult.IsSuccessful && payoutResult.Data.Amount > 0)
                    {
                        summary.ProcessedPayouts++;
                        summary.TotalAmount += payoutResult.Data.Amount;
                    }
                    else if (!payoutResult.IsSuccessful)
                    {
                        summary.FailedPayouts++;
                        summary.Errors.Add(new PayoutErrorModel
                        {
                            DriverId = driver.Id,
                            DriverName = driver.Name,
                            ErrorMessage = payoutResult.Message,
                            AttemptedAmount = 0 // We'll calculate this in the individual method
                        });
                    }
                }
                catch (Exception ex)
                {
                    summary.FailedPayouts++;
                    summary.Errors.Add(new PayoutErrorModel
                    {
                        DriverId = driver.Id,
                        DriverName = driver.Name,
                        ErrorMessage = ex.Message,
                        AttemptedAmount = 0
                    });

                    _logger.LogError(ex, "Error processing payout for driver {DriverId}", driver.Id);
                }
            }

            _logger.LogInformation("Weekly payout processing completed. Processed: {ProcessedCount}, Failed: {FailedCount}, Total Amount: ${TotalAmount}",
                summary.ProcessedPayouts, summary.FailedPayouts, summary.TotalAmount);

            return ApiResponseModel<PayoutSummaryResponseModel>.Fail(
                "Error occurred during weekly payout processing", 500);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payout for driver");
            return ApiResponseModel<PayoutSummaryResponseModel>.Fail(
                "An error occurred while processing the payout", 500);
        }
    }

    public async Task<ApiResponseModel<DriverPayoutResponseModel>> ProcessDriverPayoutAsync(string driverId, string processedBy, bool forceProcess = false)
    {
        try
        {
            var driver = await _dbContext.Drivers
                .Include(d => d.Payouts)
                .FirstOrDefaultAsync(d => d.Id == driverId);

            if (driver == null)
            {
                return ApiResponseModel<DriverPayoutResponseModel>.Fail("Driver not found", 404);
            }

            if (string.IsNullOrEmpty(driver.StripeConnectAccountId) || !driver.CanReceivePayouts)
            {
                return ApiResponseModel<DriverPayoutResponseModel>.Fail(
                    "Driver does not have a valid Stripe account for payouts", 400);
            }

            // Calculate payout period - Tuesday cutoff rule
            var (startDate, endDate) = CalculatePayoutPeriod(forceProcess);

            // Check if payout already exists for this period
            var existingPayout = await _dbContext.Set<DriverPayout>()
                .FirstOrDefaultAsync(p => p.DriverId == driverId &&
                                         p.PeriodStartDate == startDate &&
                                         p.PeriodEndDate == endDate);

            if (existingPayout != null && !forceProcess)
            {
                return ApiResponseModel<DriverPayoutResponseModel>.Success(
                    "Payout already processed for this period",
                    MapToPayoutResponse(existingPayout),
                    200);
            }

            // Get eligible completed orders for this driver in the payout period
            var eligibleOrders = await GetEligibleOrdersForPayout(driverId, startDate, endDate);

            if (!eligibleOrders.Any())
            {
                return ApiResponseModel<DriverPayoutResponseModel>.Success(
                    "No eligible orders for payout",
                    new DriverPayoutResponseModel
                    {
                        DriverId = driverId,
                        DriverName = driver.Name,
                        Amount = 0,
                        Currency = "usd",
                        PeriodStartDate = startDate,
                        PeriodEndDate = endDate,
                        Status = PayoutStatus.Completed,
                        OrdersCount = 0
                    },
                    200);
            }

            // Calculate total earnings
            var totalEarnings = eligibleOrders.Sum(o => o.DriverEarnings ?? 0);

            if (totalEarnings <= 0)
            {
                return ApiResponseModel<DriverPayoutResponseModel>.Fail(
                    "No earnings found for payout period", 400);
            }

            // Create payout record
            var payout = new DriverPayout
            {
                Id = Guid.NewGuid().ToString(),
                DriverId = driverId,
                Amount = totalEarnings,
                Currency = "usd",
                PeriodStartDate = startDate,
                PeriodEndDate = endDate,
                ProcessedDate = DateTime.UtcNow,
                Status = PayoutStatus.Processing,
                OrdersCount = eligibleOrders.Count,
                ProcessedBy = processedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create payout order records
            foreach (var order in eligibleOrders)
            {
                payout.PayoutOrders.Add(new DriverPayoutOrder
                {
                    Id = Guid.NewGuid().ToString(),
                    DriverPayoutId = payout.Id,
                    OrderId = order.Id,
                    OrderEarnings = order.DriverEarnings ?? 0,
                    OrderCompletedDate = order.UpdatedAt, // Assuming UpdatedAt represents completion
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            _dbContext.Set<DriverPayout>().Add(payout);
            await _dbContext.SaveChangesAsync();

            // Process Stripe transfer
            var transferResult = await _stripeConnectService.TransferToDriverAsync(
                driverId,
                totalEarnings,
                $"Weekly payout for period {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                null);

            if (transferResult.IsSuccessful)
            {
                payout.Status = PayoutStatus.Completed;
                await _dbContext.SaveChangesAsync();

                // Send notification to driver
                await _notificationService.CreateNotificationAsync(
                    driver.UserId,
                    "Payout Processed",
                    $"Your weekly payout of ${totalEarnings:F2} has been processed and will arrive in your account soon.",
                    NotificationType.PaymentNotification,
                    payout.Id,
                    "DriverPayout");

                _logger.LogInformation("Successfully processed payout of ${Amount} for driver {DriverId}",
                    totalEarnings, driverId);
            }
            else
            {
                payout.Status = PayoutStatus.Failed;
                payout.FailureReason = transferResult.Message;
                await _dbContext.SaveChangesAsync();

                _logger.LogError("Failed to process Stripe transfer for driver {DriverId}: {Error}",
                    driverId, transferResult.Message);
            }

            return ApiResponseModel<DriverPayoutResponseModel>.Success(
                payout.Status == PayoutStatus.Completed ? "Payout processed successfully" : "Payout failed",
                MapToPayoutResponse(payout),
                payout.Status == PayoutStatus.Completed ? 200 : 400);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payout for driver {DriverId}", driverId);
            return ApiResponseModel<DriverPayoutResponseModel>.Fail(
                "An error occurred while processing the payout", 500);
        }
    }

    public async Task<ApiResponseModel<List<DriverPayoutResponseModel>>> GetDriverPayoutHistoryAsync(string driverId, int page = 1, int pageSize = 10)
    {
        try
        {
            var payouts = await _dbContext.Set<DriverPayout>()
                .Include(p => p.Driver)
                .Include(p => p.PayoutOrders)
                    .ThenInclude(po => po.Order)
                .Where(p => p.DriverId == driverId)
                .OrderByDescending(p => p.ProcessedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var payoutResponses = payouts.Select(MapToPayoutResponse).ToList();

            return ApiResponseModel<List<DriverPayoutResponseModel>>.Success(
                "Payout history retrieved successfully",
                payoutResponses,
                200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payout history for driver {DriverId}", driverId);
            return ApiResponseModel<List<DriverPayoutResponseModel>>.Fail(
                "Error retrieving payout history", 500);
        }
    }

    public async Task<ApiResponseModel<DriverEarningsProjectionModel>> GetDriverEarningsProjectionAsync(string driverId)
    {
        try
        {
            var driver = await _dbContext.Drivers.FirstOrDefaultAsync(d => d.Id == driverId);
            if (driver == null)
            {
                return ApiResponseModel<DriverEarningsProjectionModel>.Fail("Driver not found", 404);
            }

            var (currentPeriodStart, currentPeriodEnd) = CalculatePayoutPeriod(false);
            var (nextPeriodStart, nextPeriodEnd) = CalculateNextPayoutPeriod();

            // Get current week earnings (completed orders)
            var currentWeekOrders = await GetEligibleOrdersForPayout(driverId, currentPeriodStart, currentPeriodEnd);
            var currentWeekEarnings = currentWeekOrders.Sum(o => o.DriverEarnings ?? 0);

            // Get pending orders that will be in next payout
            var pendingOrders = await _dbContext.Set<CargoOrders>()
                .Where(o => o.AcceptedBid != null &&
                           o.AcceptedBid.Truck != null &&
                           o.AcceptedBid.Truck.DriverId == driverId &&
                           o.Status == CargoOrderStatus.Completed &&
                           !o.IsFlagged &&
                           o.UpdatedAt >= nextPeriodStart &&
                           o.UpdatedAt <= nextPeriodEnd)
                .ToListAsync();

            var nextPayoutAmount = pendingOrders.Sum(o => o.DriverEarnings ?? 0);

            var projection = new DriverEarningsProjectionModel
            {
                DriverId = driverId,
                DriverName = driver.Name,
                CurrentWeekEarnings = currentWeekEarnings,
                NextPayoutAmount = nextPayoutAmount,
                NextPayoutDate = GetNextFriday(),
                PendingOrdersCount = pendingOrders.Count,
                PendingOrders = pendingOrders.Select(o => new PendingOrderEarningsModel
                {
                    OrderId = o.Id,
                    Earnings = o.DriverEarnings ?? 0,
                    CompletedDate = o.UpdatedAt,
                    IsFlagged = o.IsFlagged,
                    FlagReason = o.FlagReason
                }).ToList()
            };

            return ApiResponseModel<DriverEarningsProjectionModel>.Success(
                "Earnings projection calculated successfully",
                projection,
                200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating earnings projection for driver {DriverId}", driverId);
            return ApiResponseModel<DriverEarningsProjectionModel>.Fail(
                "Error calculating earnings projection", 500);
        }
    }

    public async Task<ApiResponseModel<List<DriverEarningsProjectionModel>>> GetAllDriverEarningsProjectionsAsync()
    {
        try
        {
            var activeDrivers = await _dbContext.Drivers
                .Where(d => d.IsActive)
                .ToListAsync();

            var projections = new List<DriverEarningsProjectionModel>();

            foreach (var driver in activeDrivers)
            {
                var projection = await GetDriverEarningsProjectionAsync(driver.Id);
                if (projection.IsSuccessful)
                {
                    projections.Add(projection.Data);
                }
            }

            return ApiResponseModel<List<DriverEarningsProjectionModel>>.Success(
                "All driver earnings projections calculated",
                projections,
                200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating all driver earnings projections");
            return ApiResponseModel<List<DriverEarningsProjectionModel>>.Fail(
                "Error calculating earnings projections", 500);
        }
    }

    public async Task<ApiResponseModel<bool>> RecalculateDriverEarningsAsync(string orderId)
    {
        try
        {
            var order = await _dbContext.Set<CargoOrders>()
                .Include(o => o.AcceptedBid)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order?.AcceptedBid == null)
            {
                return ApiResponseModel<bool>.Fail("Order or accepted bid not found", 404);
            }

            // Calculate driver earnings (bid amount minus platform fee)
            var platformFeePercentage = 0.15m; // 15% platform fee - adjust as needed
            var driverEarnings = order.AcceptedBid.Amount * (1 - platformFeePercentage);

            order.DriverEarnings = driverEarnings;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Recalculated driver earnings for order {OrderId}: ${Earnings}",
                orderId, driverEarnings);

            return ApiResponseModel<bool>.Success("Driver earnings recalculated successfully", true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating driver earnings for order {OrderId}", orderId);
            return ApiResponseModel<bool>.Fail("Error recalculating driver earnings", 500);
        }
    }

    private async Task<List<CargoOrders>> GetEligibleOrdersForPayout(string driverId, DateTime startDate, DateTime endDate)
    {
        return await _dbContext.Set<CargoOrders>()
            .Include(o => o.AcceptedBid)
                .ThenInclude(b => b.Truck)
            .Where(o => o.AcceptedBid != null &&
                       o.AcceptedBid.Truck != null &&
                       o.AcceptedBid.Truck.DriverId == driverId &&
                       o.Status == CargoOrderStatus.Completed &&
                       !o.IsFlagged &&
                       o.UpdatedAt >= startDate &&
                       o.UpdatedAt <= endDate &&
                       o.DriverEarnings.HasValue &&
                       o.DriverEarnings > 0)
            .ToListAsync();
    }

    private static (DateTime startDate, DateTime endDate) CalculatePayoutPeriod(bool forceProcess)
    {
        var today = DateTime.UtcNow.Date;
        var dayOfWeek = today.DayOfWeek;

        if (!forceProcess && dayOfWeek != DayOfWeek.Friday)
        {
            // If not Friday and not forced, calculate the previous payout period
            var lastFriday = today.AddDays(-(int)dayOfWeek - 2);
            if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
            {
                lastFriday = today.AddDays(-(int)dayOfWeek + 5);
            }

            var periodEnd = lastFriday.AddDays(-5); // Tuesday of the previous week
            var periodStart = periodEnd.AddDays(-6); // Wednesday of the week before

            return (periodStart, periodEnd);
        }

        // For Friday processing or forced processing
        // Orders completed from last Wednesday to this Tuesday are eligible
        var currentTuesday = today.AddDays(-(int)dayOfWeek + 2);
        if (dayOfWeek <= DayOfWeek.Tuesday)
        {
            currentTuesday = currentTuesday.AddDays(-7); // Previous Tuesday if we haven't reached this week's Tuesday
        }

        var previousWednesday = currentTuesday.AddDays(-6);

        return (previousWednesday, currentTuesday);
    }

    private static (DateTime startDate, DateTime endDate) CalculateNextPayoutPeriod()
    {
        var today = DateTime.UtcNow.Date;
        var nextTuesday = GetNextTuesday();
        var currentWednesday = nextTuesday.AddDays(-6);

        return (currentWednesday, nextTuesday);
    }

    private static DateTime GetNextFriday()
    {
        var today = DateTime.UtcNow.Date;
        var daysUntilFriday = ((int)DayOfWeek.Friday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilFriday == 0 && DateTime.UtcNow.Hour >= 1) // If it's Friday and past 1 AM
        {
            daysUntilFriday = 7; // Next Friday
        }
        return today.AddDays(daysUntilFriday);
    }

    private static DateTime GetNextTuesday()
    {
        var today = DateTime.UtcNow.Date;
        var daysUntilTuesday = ((int)DayOfWeek.Tuesday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilTuesday == 0) // If it's Tuesday
        {
            daysUntilTuesday = 7; // Next Tuesday
        }
        return today.AddDays(daysUntilTuesday);
    }

    private DriverPayoutResponseModel MapToPayoutResponse(DriverPayout payout)
    {
        return new DriverPayoutResponseModel
        {
            Id = payout.Id,
            DriverId = payout.DriverId,
            DriverName = payout.Driver?.Name ?? "Unknown",
            Amount = payout.Amount,
            Currency = payout.Currency,
            PeriodStartDate = payout.PeriodStartDate,
            PeriodEndDate = payout.PeriodEndDate,
            ProcessedDate = payout.ProcessedDate,
            Status = payout.Status,
            OrdersCount = payout.OrdersCount,
            StripeTransferId = payout.StripeTransferId,
            FailureReason = payout.FailureReason,
            Orders = payout.PayoutOrders?.Select(po => new PayoutOrderDetailModel
            {
                OrderId = po.OrderId,
                OrderEarnings = po.OrderEarnings,
                CompletedDate = po.OrderCompletedDate,
                PickupLocation = po.Order?.PickupLocation ?? "Unknown",
                DeliveryLocation = po.Order?.DeliveryLocation ?? "Unknown"
            }).ToList() ?? new List<PayoutOrderDetailModel>()
        };
    }
}