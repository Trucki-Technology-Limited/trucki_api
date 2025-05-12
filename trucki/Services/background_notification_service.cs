using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using trucki.DatabaseContext;
using trucki.Entities;

namespace trucki.Services
{
    public class BackgroundNotificationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BackgroundNotificationService> _logger;

        public BackgroundNotificationService(
            IServiceScopeFactory scopeFactory,
            ILogger<BackgroundNotificationService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Notification Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Run notifications checks every hour
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);

                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<TruckiDBContext>();
                    var notificationEventService = scope.ServiceProvider.GetRequiredService<NotificationEventService>();

                    _logger.LogInformation("Running scheduled notification checks");

                    // Check for upcoming pickups (24 hours notice)
                    await CheckUpcomingPickups(dbContext, notificationEventService);

                    // Check for payment reminders (3 days before due date)
                    await CheckPaymentReminders(dbContext, notificationEventService);

                    // Check for overdue invoices
                    await CheckOverdueInvoices(dbContext, notificationEventService);
                }
                catch (OperationCanceledException)
                {
                    // Normal shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in background notification service");

                    // Wait a bit before retrying after an error
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("Background Notification Service stopped");
        }

        private async Task CheckUpcomingPickups(TruckiDBContext dbContext, NotificationEventService notificationService)
        {
            try
            {
                var nextDay = DateTime.UtcNow.AddHours(24);

                var upcomingPickups = await dbContext.Set<CargoOrders>()
                    .Include(o => o.AcceptedBid)
                        .ThenInclude(b => b.Truck)
                            .ThenInclude(t => t.Driver)
                    .Where(o =>
                        o.Status == CargoOrderStatus.DriverAcknowledged &&
                        o.PickupDateTime.HasValue &&
                        o.PickupDateTime.Value < nextDay &&
                        o.PickupDateTime.Value > DateTime.UtcNow &&
                        o.AcceptedBid != null &&
                        o.AcceptedBid.Truck != null &&
                        o.AcceptedBid.Truck.Driver != null)
                    .ToListAsync();

                _logger.LogInformation($"Found {upcomingPickups.Count} upcoming pickups for notification");

                foreach (var order in upcomingPickups)
                {
                    await notificationService.NotifyPickupReminder(
                        order.AcceptedBid.Truck.Driver.Id,
                        order.Id,
                        order.PickupLocation,
                        order.PickupDateTime.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking upcoming pickups");
            }
        }

        private async Task CheckPaymentReminders(TruckiDBContext dbContext, NotificationEventService notificationService)
        {
            try
            {
                var reminderDate = DateTime.UtcNow.AddDays(3);

                var upcomingPayments = await dbContext.Set<Invoice>()
                    .Include(i => i.Order)
                    .Where(i =>
                        i.Status == InvoiceStatus.Pending &&
                        i.DueDate < reminderDate &&
                        i.DueDate > DateTime.UtcNow)
                    .ToListAsync();

                _logger.LogInformation($"Found {upcomingPayments.Count} upcoming payments for reminder");

                foreach (var invoice in upcomingPayments)
                {
                    await notificationService.NotifyPaymentReminder(
                        invoice.Order.CargoOwnerId,
                        invoice.Order.Id,
                        invoice.InvoiceNumber,
                        invoice.TotalAmount,
                        invoice.DueDate);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking payment reminders");
            }
        }

        private async Task CheckOverdueInvoices(TruckiDBContext dbContext, NotificationEventService notificationService)
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                var overdueInvoices = await dbContext.Set<Invoice>()
                    .Include(i => i.Order)
                    .Where(i =>
                        i.Status == InvoiceStatus.Pending &&
                        i.DueDate.Date < today)
                    .ToListAsync();

                _logger.LogInformation($"Found {overdueInvoices.Count} overdue invoices");

                foreach (var invoice in overdueInvoices)
                {
                    // Update status to overdue
                    invoice.Status = InvoiceStatus.Overdue;

                    // Use the NotificationEventService to send the overdue notification
                    await notificationService.NotifyPaymentReminder(
                        invoice.Order.CargoOwnerId,
                        invoice.Order.Id,
                        invoice.InvoiceNumber,
                        invoice.TotalAmount,
                        invoice.DueDate);
                }

                // Save changes for status updates
                if (overdueInvoices.Any())
                {
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking overdue invoices");
            }
        }
    }
}