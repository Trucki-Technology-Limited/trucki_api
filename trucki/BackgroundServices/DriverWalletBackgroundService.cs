using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using trucki.Interfaces.IServices;

namespace trucki.BackgroundServices
{
    public class DriverWalletBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DriverWalletBackgroundService> _logger;

        public DriverWalletBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<DriverWalletBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Driver Wallet Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                // Check time to see if we need to run any jobs
                var now = DateTime.UtcNow;
                
                // Update projections every day at midnight
                if (now.Hour == 0 && now.Minute == 0)
                {
                    await UpdateWithdrawalProjectionsAsync();
                }
                
                // Process withdrawals on Friday at 1 AM
                if (now.DayOfWeek == DayOfWeek.Friday && now.Hour == 1 && now.Minute == 0)
                {
                    await ProcessWeeklyWithdrawalsAsync();
                }
                
                // Wait for a minute before checking again
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task UpdateWithdrawalProjectionsAsync()
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var driverWalletService = scope.ServiceProvider.GetRequiredService<IDriverWalletService>();
                    
                    _logger.LogInformation("Running scheduled task: Update Driver Withdrawal Projections");
                    
                    var result = await driverWalletService.UpdateWithdrawalProjectionsAsync();
                    
                    if (result.IsSuccessful)
                    {
                        _logger.LogInformation("Successfully updated driver withdrawal projections");
                    }
                    else
                    {
                        _logger.LogWarning("Failed to update driver withdrawal projections. Status Code: {StatusCode}, Message: {Message}",
                            result.StatusCode, result.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating driver withdrawal projections");
            }
        }

        private async Task ProcessWeeklyWithdrawalsAsync()
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var driverWalletService = scope.ServiceProvider.GetRequiredService<IDriverWalletService>();
                    
                    _logger.LogInformation("Running scheduled task: Process Weekly Driver Withdrawals");
                    
                    // Use a system ID for the admin identifier
                    var systemAdminId = "system-auto-withdrawal";
                    
                    var result = await driverWalletService.ProcessWeeklyWithdrawalsAsync(systemAdminId);
                    
                    if (result.IsSuccessful)
                    {
                        _logger.LogInformation("Successfully processed {Count} driver withdrawals for a total of {Amount:C}",
                            result.Data.ProcessedCount, result.Data.TotalAmount);
                        
                        if (result.Data.Errors.Count > 0)
                        {
                            _logger.LogWarning("There were {Count} errors during withdrawal processing", 
                                result.Data.Errors.Count);
                            
                            foreach (var error in result.Data.Errors)
                            {
                                _logger.LogWarning("Error processing withdrawal for driver {DriverName} ({DriverId}): {Message}",
                                    error.DriverName, error.DriverId, error.ErrorMessage);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Failed to process driver withdrawals. Status Code: {StatusCode}, Message: {Message}",
                            result.StatusCode, result.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing weekly driver withdrawals");
            }
        }
    }
}