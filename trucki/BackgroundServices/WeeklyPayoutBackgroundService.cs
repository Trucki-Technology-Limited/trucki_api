using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.ResponseModels;

namespace trucki.BackgroundServices
{
    public class WeeklyPayoutBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WeeklyPayoutBackgroundService> _logger;

        public WeeklyPayoutBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<WeeklyPayoutBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Weekly Payout Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;

                    // Process payouts every Friday at 2 AM UTC
                    if (now.DayOfWeek == DayOfWeek.Friday && now.Hour == 2 && now.Minute == 0)
                    {
                        await ProcessWeeklyPayoutsAsync();
                    }

                    // Check for failed payouts to retry every day at 3 AM UTC
                    if (now.Hour == 3 && now.Minute == 0)
                    {
                        await RetryFailedPayoutsAsync();
                    }

                    // Update driver earnings projections every day at midnight UTC
                    if (now.Hour == 0 && now.Minute == 30)
                    {
                        await UpdateEarningsProjectionsAsync();
                    }

                    // Wait for a minute before checking again
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Normal shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Weekly Payout Background Service");

                    // Wait 5 minutes before retrying after an error
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("Weekly Payout Background Service stopped.");
        }

        private async Task ProcessWeeklyPayoutsAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                // Use integrated service if available, fallback to individual services
                var integratedService = scope.ServiceProvider.GetService<IIntegratedPayoutService>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                _logger.LogInformation("Starting automatic weekly payout processing");

                if (integratedService != null)
                {
                    // Use integrated approach (handles both Stripe Connect and Wallet)
                    var result = await integratedService.ProcessWeeklyPayoutsAsync("system-auto-payout");

                    if (result.IsSuccessful)
                    {
                        _logger.LogInformation("Integrated weekly payouts processed successfully. " +
                            "Stripe: {StripeCount}, Wallet: {WalletCount}, Failed: {FailedCount}",
                            result.Data.StripePayouts, result.Data.WalletPayouts, result.Data.FailedPayouts);

                        await SendIntegratedPayoutSummaryAsync(result.Data);
                    }
                    else
                    {
                        _logger.LogError("Integrated payout processing failed: {Message}", result.Message);
                        await SendAdminErrorNotificationAsync("Integrated Payout Processing Failed", result.Message);
                    }
                }
                else
                {
                    // Fallback to original Stripe-only approach
                    var payoutService = scope.ServiceProvider.GetRequiredService<IDriverPayoutService>();
                    var result = await payoutService.ProcessWeeklyPayoutsAsync("system-auto-payout");

                    if (result.IsSuccessful)
                    {
                        _logger.LogInformation("Weekly payouts processed successfully. " +
                            "Processed: {ProcessedCount}, Failed: {FailedCount}, Total: ${TotalAmount}",
                            result.Data.ProcessedPayouts, result.Data.FailedPayouts, result.Data.TotalAmount);

                        if (result.Data.ProcessedPayouts > 0 || result.Data.FailedPayouts > 0)
                        {
                            await SendAdminPayoutSummaryAsync(result.Data);
                        }
                    }
                    else
                    {
                        _logger.LogError("Weekly payout processing failed: {Message}", result.Message);
                        await SendAdminErrorNotificationAsync("Weekly Payout Processing Failed", result.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during automatic weekly payout processing");
                await SendAdminErrorNotificationAsync("Weekly Payout Service Error", ex.Message);
            }
        }

        private async Task SendAdminPayoutSummaryAsync(PayoutSummaryResponseModel summary)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var message = $"Weekly payout processing completed:\n" +
                             $"• Processed: {summary.ProcessedPayouts} payouts\n" +
                             $"• Failed: {summary.FailedPayouts} payouts\n" +
                             $"• Total Amount: ${summary.TotalAmount:F2}\n" +
                             $"• Eligible Drivers: {summary.EligibleDrivers}/{summary.TotalDrivers}";

                // Send email notification to admin (configure admin email in settings)
                var adminEmail = scope.ServiceProvider.GetRequiredService<IConfiguration>()
                    .GetValue<string>("AdminNotifications:Email");

                if (!string.IsNullOrEmpty(adminEmail))
                {
                    await emailService.SendEmailAsync(
                        adminEmail,
                        "Weekly Payout Summary",
                        $"<h2>Weekly Payout Processing Summary</h2>" +
                        $"<p><strong>Date:</strong> {summary.ProcessedAt:yyyy-MM-dd HH:mm} UTC</p>" +
                        $"<p><strong>Processed Payouts:</strong> {summary.ProcessedPayouts}</p>" +
                        $"<p><strong>Failed Payouts:</strong> {summary.FailedPayouts}</p>" +
                        $"<p><strong>Total Amount:</strong> ${summary.TotalAmount:F2}</p>" +
                        $"<p><strong>Eligible Drivers:</strong> {summary.EligibleDrivers} out of {summary.TotalDrivers}</p>" +
                        (summary.Errors.Any() ?
                            $"<h3>Errors:</h3><ul>{string.Join("", summary.Errors.Select(e => $"<li>{e.DriverName}: {e.ErrorMessage}</li>"))}</ul>"
                            : ""));
                }

                _logger.LogInformation("Sent payout summary notification to admin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending admin payout summary notification");
            }
        }

        private async Task RetryFailedPayoutsAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TruckiDBContext>();
                var payoutService = scope.ServiceProvider.GetRequiredService<IDriverPayoutService>();

                _logger.LogInformation("Checking for failed payouts to retry");

                // Get failed payouts from the last 7 days
                var cutoffDate = DateTime.UtcNow.AddDays(-7);
                var failedPayouts = await dbContext.Set<DriverPayout>()
                    .Where(p => p.Status == PayoutStatus.Failed &&
                               p.ProcessedDate >= cutoffDate)
                    .ToListAsync();

                if (!failedPayouts.Any())
                {
                    _logger.LogInformation("No failed payouts found for retry");
                    return;
                }

                _logger.LogInformation("Found {Count} failed payouts to retry", failedPayouts.Count);

                int retrySuccessCount = 0;
                int retryFailCount = 0;

                foreach (var failedPayout in failedPayouts)
                {
                    try
                    {
                        // Try to process the driver payout again
                        var retryResult = await payoutService.ProcessDriverPayoutAsync(
                            failedPayout.DriverId,
                            "system-retry",
                            true);

                        if (retryResult.IsSuccessful && retryResult.Data.Status == PayoutStatus.Completed)
                        {
                            retrySuccessCount++;
                            _logger.LogInformation("Successfully retried payout for driver {DriverId}",
                                failedPayout.DriverId);

                            // Mark the old failed payout as cancelled
                            failedPayout.Status = PayoutStatus.Cancelled;
                            failedPayout.Notes = $"Cancelled due to successful retry at {DateTime.UtcNow}";
                        }
                        else
                        {
                            retryFailCount++;
                            _logger.LogWarning("Retry failed for driver {DriverId}: {Message}",
                                failedPayout.DriverId, retryResult.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        retryFailCount++;
                        _logger.LogError(ex, "Error retrying payout for driver {DriverId}", failedPayout.DriverId);
                    }
                }

                await dbContext.SaveChangesAsync();

                _logger.LogInformation("Payout retry completed. Success: {SuccessCount}, Failed: {FailCount}",
                    retrySuccessCount, retryFailCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during failed payout retry process");
            }
        }

        private async Task UpdateEarningsProjectionsAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var payoutService = scope.ServiceProvider.GetRequiredService<IDriverPayoutService>();

                _logger.LogInformation("Updating driver earnings projections");

                // This could be enhanced to cache projections or update specific metrics
                var projectionsResult = await payoutService.GetAllDriverEarningsProjectionsAsync();

                if (projectionsResult.IsSuccessful)
                {
                    _logger.LogInformation("Updated earnings projections for {Count} drivers",
                        projectionsResult.Data.Count);
                }
                else
                {
                    _logger.LogWarning("Failed to update earnings projections: {Message}",
                        projectionsResult.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating earnings projections");
            }
        }

        private async Task SendIntegratedPayoutSummaryAsync(IntegratedPayoutSummaryModel summary)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var message = $"Integrated weekly payout processing completed:\n" +
                             $"• Stripe Connect Payouts: {summary.StripePayouts} (${summary.TotalStripeAmount:F2})\n" +
                             $"• Wallet Payouts: {summary.WalletPayouts} (${summary.TotalWalletAmount:F2})\n" +
                             $"• Failed Payouts: {summary.FailedPayouts}\n" +
                             $"• Skipped Drivers: {summary.SkippedDrivers}";

                var adminEmail = scope.ServiceProvider.GetRequiredService<IConfiguration>()
                    .GetValue<string>("AdminNotifications:Email");

                if (!string.IsNullOrEmpty(adminEmail))
                {
                    await emailService.SendEmailAsync(
                        adminEmail,
                        "Integrated Weekly Payout Summary",
                        $"<h2>Integrated Weekly Payout Summary</h2>" +
                        $"<p><strong>Date:</strong> {summary.ProcessedAt:yyyy-MM-dd HH:mm} UTC</p>" +
                        $"<h3>Stripe Connect Payouts</h3>" +
                        $"<p><strong>Count:</strong> {summary.StripePayouts}</p>" +
                        $"<p><strong>Amount:</strong> ${summary.TotalStripeAmount:F2}</p>" +
                        $"<h3>Wallet Payouts</h3>" +
                        $"<p><strong>Count:</strong> {summary.WalletPayouts}</p>" +
                        $"<p><strong>Amount:</strong> ${summary.TotalWalletAmount:F2}</p>" +
                        $"<h3>Summary</h3>" +
                        $"<p><strong>Failed Payouts:</strong> {summary.FailedPayouts}</p>" +
                        $"<p><strong>Skipped Drivers:</strong> {summary.SkippedDrivers}</p>" +
                        $"<p><strong>Total Amount Processed:</strong> ${(summary.TotalStripeAmount + summary.TotalWalletAmount):F2}</p>" +
                        (summary.Errors.Any() ?
                            $"<h3>Errors:</h3><ul>{string.Join("", summary.Errors.Select(e => $"<li>{e.DriverName}: {e.ErrorMessage}</li>"))}</ul>"
                            : ""));
                }

                _logger.LogInformation("Sent integrated payout summary notification to admin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending integrated payout summary notification");
            }
        }

        private async Task SendAdminErrorNotificationAsync(string title, string errorMessage)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var adminEmail = scope.ServiceProvider.GetRequiredService<IConfiguration>()
                    .GetValue<string>("AdminNotifications:Email");

                if (!string.IsNullOrEmpty(adminEmail))
                {
                    await emailService.SendEmailAsync(
                        adminEmail,
                        $"Trucki Alert: {title}",
                        $"<h2>System Alert</h2>" +
                        $"<p><strong>Title:</strong> {title}</p>" +
                        $"<p><strong>Time:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>" +
                        $"<p><strong>Error:</strong> {errorMessage}</p>" +
                        $"<p>Please check the system logs for more details.</p>");
                }

                _logger.LogInformation("Sent error notification to admin: {Title}", title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending admin error notification");
            }
        }
    }
}