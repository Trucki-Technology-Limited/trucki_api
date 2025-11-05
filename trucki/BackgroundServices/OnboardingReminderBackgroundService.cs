using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IServices;

namespace trucki.BackgroundServices
{
    /// <summary>
    /// Background service that sends reminder emails to drivers who haven't completed their onboarding
    /// - First reminder: 4 hours after starting onboarding
    /// - Second reminder: 24 hours after starting onboarding
    /// </summary>
    public class OnboardingReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OnboardingReminderBackgroundService> _logger;

        public OnboardingReminderBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<OnboardingReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Onboarding Reminder Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Run every 30 minutes
                    await ProcessOnboardingRemindersAsync();
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Normal shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Onboarding Reminder Background Service");
                    // Wait 5 minutes before retrying after an error
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("Onboarding Reminder Background Service stopped.");
        }

        private async Task ProcessOnboardingRemindersAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TruckiDBContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var driverDocumentService = scope.ServiceProvider.GetRequiredService<IDriverDocumentService>();

                _logger.LogInformation("Starting onboarding reminder check");

                var now = DateTime.UtcNow;
                var fourHoursAgo = now.AddHours(-4);
                var twentyFourHoursAgo = now.AddHours(-24);

                // Get drivers with pending onboarding
                var driversWithPendingOnboarding = await dbContext.Drivers
                    .Include(d => d.User)
                    .Where(d => d.OnboardingStatus == DriverOnboardingStatus.OboardingPending && d.User != null)
                    .ToListAsync();

                if (!driversWithPendingOnboarding.Any())
                {
                    _logger.LogInformation("No drivers with pending onboarding found");
                    return;
                }

                _logger.LogInformation("Found {Count} drivers with pending onboarding", driversWithPendingOnboarding.Count);

                int firstRemindersSent = 0;
                int secondRemindersSent = 0;

                foreach (var driver in driversWithPendingOnboarding)
                {
                    try
                    {
                        // Get or create tracking record
                        var tracking = await dbContext.OnboardingReminderTrackings
                            .FirstOrDefaultAsync(t => t.DriverId == driver.Id);

                        if (tracking == null)
                        {
                            // Create new tracking record using driver's CreatedAt as onboarding start
                            tracking = new OnboardingReminderTracking
                            {
                                DriverId = driver.Id,
                                OnboardingStartedAt = driver.CreatedAt
                            };
                            dbContext.OnboardingReminderTrackings.Add(tracking);
                            await dbContext.SaveChangesAsync();
                        }

                        // Check if we need to send first reminder (4 hours after start)
                        if (!tracking.FirstReminderSent && tracking.OnboardingStartedAt <= fourHoursAgo)
                        {
                            await SendFirstReminderAsync(driver, emailService, driverDocumentService);
                            tracking.FirstReminderSent = true;
                            tracking.FirstReminderSentAt = now;
                            await dbContext.SaveChangesAsync();
                            firstRemindersSent++;
                            _logger.LogInformation("Sent first reminder to driver {DriverId} ({Email})", driver.Id, driver.EmailAddress);
                        }
                        // Check if we need to send second reminder (24 hours after start)
                        else if (!tracking.SecondReminderSent && tracking.OnboardingStartedAt <= twentyFourHoursAgo)
                        {
                            await SendSecondReminderAsync(driver, emailService, driverDocumentService);
                            tracking.SecondReminderSent = true;
                            tracking.SecondReminderSentAt = now;
                            await dbContext.SaveChangesAsync();
                            secondRemindersSent++;
                            _logger.LogInformation("Sent second reminder to driver {DriverId} ({Email})", driver.Id, driver.EmailAddress);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing onboarding reminder for driver {DriverId}", driver.Id);
                    }
                }

                _logger.LogInformation("Onboarding reminder check completed. First reminders sent: {FirstCount}, Second reminders sent: {SecondCount}",
                    firstRemindersSent, secondRemindersSent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during onboarding reminder processing");
            }
        }

        private async Task SendFirstReminderAsync(Driver driver, IEmailService emailService, IDriverDocumentService driverDocumentService)
        {
            var pendingItems = await GetPendingOnboardingItemsAsync(driver, driverDocumentService);
            await emailService.SendFirstOnboardingReminderAsync(driver.EmailAddress, driver.Name, pendingItems);
        }

        private async Task SendSecondReminderAsync(Driver driver, IEmailService emailService, IDriverDocumentService driverDocumentService)
        {
            var pendingItems = await GetPendingOnboardingItemsAsync(driver, driverDocumentService);
            await emailService.SendSecondOnboardingReminderAsync(driver.EmailAddress, driver.Name, pendingItems);
        }

        /// <summary>
        /// Determines what onboarding items are still pending for a driver
        /// </summary>
        private async Task<List<string>> GetPendingOnboardingItemsAsync(Driver driver, IDriverDocumentService driverDocumentService)
        {
            var pendingItems = new List<string>();

            // Check basic profile information
            if (string.IsNullOrEmpty(driver.Phone))
            {
                pendingItems.Add("Phone number verification");
            }

            if (string.IsNullOrEmpty(driver.DriversLicence))
            {
                pendingItems.Add("Driver's license upload");
            }

            // Check DOT Number for US drivers
            if (driver.Country?.ToUpper() == "US" && string.IsNullOrEmpty(driver.DotNumber))
            {
                pendingItems.Add("DOT Number");
            }

            // Check truck assignment
            if (string.IsNullOrEmpty(driver.TruckId))
            {
                pendingItems.Add("Truck assignment or registration");
            }

            // Check required documents
            try
            {
                var documentSummary = await driverDocumentService.GetDriverDocumentSummaryAsync(driver.Id);
                var pendingDocuments = documentSummary
                    .Where(d => d.IsRequired && (d.ApprovalStatus == "NotUploaded" || d.ApprovalStatus == "Rejected"))
                    .ToList();

                foreach (var doc in pendingDocuments)
                {
                    if (doc.ApprovalStatus == "NotUploaded")
                    {
                        pendingItems.Add($"Upload {doc.DocumentTypeName}");
                    }
                    else if (doc.ApprovalStatus == "Rejected")
                    {
                        pendingItems.Add($"Re-upload {doc.DocumentTypeName} (previous submission was rejected)");
                    }
                }

                // Check for pending documents (uploaded but not yet reviewed)
                var pendingReview = documentSummary
                    .Where(d => d.IsRequired && d.ApprovalStatus == "Pending")
                    .ToList();

                if (pendingReview.Any())
                {
                    pendingItems.Add($"Document review in progress ({pendingReview.Count} document(s) pending approval)");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking documents for driver {DriverId}", driver.Id);
            }

            // If no specific items identified, add generic message
            if (!pendingItems.Any())
            {
                pendingItems.Add("Complete your driver profile");
                pendingItems.Add("Upload all required documents");
            }

            return pendingItems;
        }
    }
}
