using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IServices;

namespace trucki.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly TruckiDBContext _dbContext;
        private readonly IStripeConnectService _stripeConnectService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(
            IConfiguration configuration,
            TruckiDBContext dbContext,
            IStripeConnectService stripeConnectService,
            INotificationService notificationService,
            ILogger<StripeWebhookController> logger)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _stripeConnectService = stripeConnectService;
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var endpointSecret = _configuration.GetValue<string>("Stripe:WebhookSecret");

            try
            {
                Event stripeEvent;
                
                if (!string.IsNullOrEmpty(endpointSecret))
                {
                    // Verify webhook signature
                    var signatureHeader = Request.Headers["Stripe-Signature"].FirstOrDefault();
                    stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, endpointSecret);
                }
                else
                {
                    // For testing without webhook secret
                    stripeEvent = EventUtility.ParseEvent(json);
                    _logger.LogWarning("Webhook processed without signature verification. Set Stripe:WebhookSecret in production.");
                }

                _logger.LogInformation("Received Stripe webhook: {EventType} for {ObjectId}", 
                    stripeEvent.Type, stripeEvent.Data?.Object);

                // Handle different event types
                switch (stripeEvent.Type)
                {
                    case "account.updated":
                        await HandleAccountUpdated(stripeEvent);
                        break;

                    case "transfer.created":
                        await HandleTransferCreated(stripeEvent);
                        break;

                    case "transfer.paid":
                        await HandleTransferPaid(stripeEvent);
                        break;

                    case "transfer.failed":
                        await HandleTransferFailed(stripeEvent);
                        break;

                    case "payout.paid":
                        await HandlePayoutPaid(stripeEvent);
                        break;

                    case "payout.failed":
                        await HandlePayoutFailed(stripeEvent);
                        break;

                    default:
                        _logger.LogInformation("Unhandled webhook event type: {EventType}", stripeEvent.Type);
                        break;
                }

                return Ok(new { received = true });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook error: {Error}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook processing error");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        private async Task HandleAccountUpdated(Event stripeEvent)
        {
            try
            {
                var account = stripeEvent.Data.Object as Account;
                if (account == null) return;

                var driver = await _dbContext.Drivers
                    .FirstOrDefaultAsync(d => d.StripeConnectAccountId == account.Id);

                if (driver == null)
                {
                    _logger.LogWarning("Received account.updated webhook for unknown account: {AccountId}", account.Id);
                    return;
                }

                var previousStatus = driver.StripeAccountStatus;
                var previousCanReceivePayouts = driver.CanReceivePayouts;

                // Update driver status based on account capabilities
                driver.StripeAccountStatus = MapStripeStatus(account);
                driver.CanReceivePayouts = account.ChargesEnabled && account.PayoutsEnabled;
                driver.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                // Send notification if status changed significantly
                if (driver.CanReceivePayouts && !previousCanReceivePayouts)
                {
                    await _notificationService.CreateNotificationAsync(
                        driver.UserId,
                        "Stripe Account Activated",
                        "Your Stripe account is now active and you can receive payouts!",
                        NotificationType.AccountUpdate,
                        driver.Id,
                        "Driver");
                }
                else if (!driver.CanReceivePayouts && previousCanReceivePayouts)
                {
                    await _notificationService.CreateNotificationAsync(
                        driver.UserId,
                        "Stripe Account Issue",
                        "There's an issue with your Stripe account. Please complete your onboarding to continue receiving payouts.",
                        NotificationType.AccountUpdate,
                        driver.Id,
                        "Driver");
                }

                _logger.LogInformation("Updated driver {DriverId} Stripe status from {OldStatus} to {NewStatus}, CanReceivePayouts: {CanReceivePayouts}",
                    driver.Id, previousStatus, driver.StripeAccountStatus, driver.CanReceivePayouts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling account.updated webhook");
            }
        }

        private async Task HandleTransferCreated(Event stripeEvent)
        {
            try
            {
                var transfer = stripeEvent.Data.Object as Transfer;
                if (transfer == null) return;

                // Find the associated payout using metadata
                if (transfer.Metadata != null && transfer.Metadata.TryGetValue("driver_id", out var driverId))
                {
                    var payout = await _dbContext.Set<DriverPayout>()
                        .Where(p => p.DriverId == driverId && p.Status == PayoutStatus.Processing)
                        .OrderByDescending(p => p.ProcessedDate)
                        .FirstOrDefaultAsync();

                    if (payout != null)
                    {
                        payout.StripeTransferId = transfer.Id;
                        payout.UpdatedAt = DateTime.UtcNow;
                        await _dbContext.SaveChangesAsync();

                        _logger.LogInformation("Transfer {TransferId} created for payout {PayoutId}", 
                            transfer.Id, payout.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling transfer.created webhook");
            }
        }

        private async Task HandleTransferPaid(Event stripeEvent)
        {
            try
            {
                var transfer = stripeEvent.Data.Object as Transfer;
                if (transfer == null) return;

                var payout = await _dbContext.Set<DriverPayout>()
                    .Include(p => p.Driver)
                    .FirstOrDefaultAsync(p => p.StripeTransferId == transfer.Id);

                if (payout != null)
                {
                    payout.Status = PayoutStatus.Completed;
                    payout.UpdatedAt = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();

                    // Send success notification to driver
                    await _notificationService.CreateNotificationAsync(
                        payout.Driver.UserId,
                        "Payout Completed",
                        $"Your payout of ${payout.Amount:F2} has been successfully transferred to your account.",
                        NotificationType.PaymentNotification,
                        payout.Id,
                        "DriverPayout");

                    _logger.LogInformation("Transfer {TransferId} completed for payout {PayoutId}", 
                        transfer.Id, payout.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling transfer.paid webhook");
            }
        }

        private async Task HandleTransferFailed(Event stripeEvent)
        {
            try
            {
                var transfer = stripeEvent.Data.Object as Transfer;
                if (transfer == null) return;

                var payout = await _dbContext.Set<DriverPayout>()
                    .Include(p => p.Driver)
                    .FirstOrDefaultAsync(p => p.StripeTransferId == transfer.Id);

                if (payout != null)
                {
                    payout.Status = PayoutStatus.Failed;
                    // Note: Transfer doesn't have FailureMessage, using a generic message
                    payout.FailureReason = "Transfer failed - please check your Stripe account details";
                    payout.UpdatedAt = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();

                    // Send failure notification to driver
                    await _notificationService.CreateNotificationAsync(
                        payout.Driver.UserId,
                        "Payout Failed",
                        $"Your payout of ${payout.Amount:F2} failed. Please contact support for assistance.",
                        NotificationType.PaymentNotification,
                        payout.Id,
                        "DriverPayout");

                    _logger.LogError("Transfer {TransferId} failed for payout {PayoutId}: {Reason}", 
                        transfer.Id, payout.Id, payout.FailureReason);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling transfer.failed webhook");
            }
        }

        private async Task HandlePayoutPaid(Event stripeEvent)
        {
            try
            {
                var stripePayout = stripeEvent.Data.Object as Payout;
                if (stripePayout == null) return;

                // This is when funds actually reach the driver's bank account
                // You can use this to track final payout completion
                _logger.LogInformation("Payout {PayoutId} paid to bank account", stripePayout.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payout.paid webhook");
            }
        }

        private async Task HandlePayoutFailed(Event stripeEvent)
        {
            try
            {
                var stripePayout = stripeEvent.Data.Object as Payout;
                if (stripePayout == null) return;

                _logger.LogError("Payout {PayoutId} failed: {FailureMessage}", 
                    stripePayout.Id, stripePayout.FailureMessage ?? "Unknown error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payout.failed webhook");
            }
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
}