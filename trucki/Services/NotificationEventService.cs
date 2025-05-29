using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IServices;

namespace trucki.Services
{
    public class NotificationEventService
    {
        private readonly INotificationService _notificationService;
        private readonly TruckiDBContext _dbContext;
        private readonly ILogger<NotificationEventService> _logger;

        public NotificationEventService(
            INotificationService notificationService,
            TruckiDBContext dbContext,
            ILogger<NotificationEventService> logger)
        {
            _notificationService = notificationService;
            _dbContext = dbContext;
            _logger = logger;
        }

        // Robust wrapper to handle exceptions in notification events
        private async Task SafeExecuteNotificationAsync(Func<Task> notificationAction, string eventType)
        {
            try
            {
                await notificationAction();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in {eventType} notification: {ex.Message}");
                // Don't rethrow - allow the app to continue even if notifications fail
            }
        }

        // Order created notification for cargo owner
        public async Task NotifyOrderCreated(string cargoOwnerId, string orderId, string pickupLocation, string deliveryLocation)
        {
            await SafeExecuteNotificationAsync(async () =>
            {
                var title = "New Order Created";
                var message = $"Your order from {pickupLocation} to {deliveryLocation} has been created successfully.";

                // Get the UserId from CargoOwner
                var cargoOwner = await _dbContext.CargoOwners.FirstOrDefaultAsync(co => co.Id == cargoOwnerId);
                if (cargoOwner?.UserId != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        cargoOwner.UserId,
                        title,
                        message,
                        NotificationType.OrderCreated,
                        orderId,
                        "CargoOrders");
                }
            }, "OrderCreated");
        }

        // Order open for bidding notification for all available drivers
        public async Task NotifyOrderOpenForBidding(string orderId, string pickupLocation, string deliveryLocation)
        {
            await SafeExecuteNotificationAsync(async () =>
            {
                var title = "New Order Available for Bidding";
                var message = $"A new order from {pickupLocation} to {deliveryLocation} is open for bidding.";

                // Get all active drivers with available trucks
                var driversWithAvailableTrucks = await _dbContext.Drivers
                    .Include(d => d.Truck)
                    .Include(d => d.User)
                    .Where(d => d.IsActive && d.Truck != null && d.Truck.TruckStatus == TruckStatus.Available && d.UserId != null)
                    .Select(d => d.UserId)
                    .ToListAsync();

                if (driversWithAvailableTrucks.Any())
                {
                    await _notificationService.CreateNotificationsForMultipleUsersAsync(
                        driversWithAvailableTrucks,
                        title,
                        message,
                        NotificationType.OrderCreated,
                        orderId,
                        "CargoOrders");
                }
            }, "OrderOpenForBidding");
        }

        // Bid submitted notification for cargo owner
        public async Task NotifyBidSubmitted(string cargoOwnerId, string orderId, string driverName, decimal amount)
        {
            await SafeExecuteNotificationAsync(async () =>
            {
                var title = "New Bid Received";
                var message = $"Driver {driverName} has submitted a bid of ${amount} for your order.";

                // Get the UserId from CargoOwner
                var cargoOwner = await _dbContext.CargoOwners.FirstOrDefaultAsync(co => co.Id == cargoOwnerId);
                if (cargoOwner?.UserId != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        cargoOwner.UserId,
                        title,
                        message,
                        NotificationType.BidSubmitted,
                        orderId,
                        "CargoOrders");
                }
            }, "BidSubmitted");
        }

        // Bid accepted notification for driver
        public async Task NotifyBidAccepted(string driverId, string orderId, string pickupLocation, string deliveryLocation)
        {
            await SafeExecuteNotificationAsync(async () =>
            {
                var title = "Bid Accepted";
                var message = $"Your bid for the order from {pickupLocation} to {deliveryLocation} has been accepted.";

                // Get the UserId from Driver
                var driver = await _dbContext.Drivers.FirstOrDefaultAsync(d => d.Id == driverId);
                if (driver?.UserId != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        driver.UserId,
                        title,
                        message,
                        NotificationType.BidAccepted,
                        orderId,
                        "CargoOrders");
                }
            }, "BidAccepted");
        }

        // Driver acknowledged notification for cargo owner
        public async Task NotifyDriverAcknowledged(string cargoOwnerId, string orderId, string driverName)
        {
            await SafeExecuteNotificationAsync(async () =>
            {
                var title = "Driver Acknowledged Order";
                var message = $"Driver {driverName} has acknowledged your order and will start soon.";

                // Get the UserId from CargoOwner
                var cargoOwner = await _dbContext.CargoOwners.FirstOrDefaultAsync(co => co.Id == cargoOwnerId);
                if (cargoOwner?.UserId != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        cargoOwner.UserId,
                        title,
                        message,
                        NotificationType.OrderAssigned,
                        orderId,
                        "CargoOrders");
                }
            }, "DriverAcknowledged");
        }

        // Driver Declined notification for cargo owner
        public async Task NotifyDriverDeclined(string cargoOwnerId, string orderId, string driverName)
        {
            await SafeExecuteNotificationAsync(async () =>
            {
                var title = "Driver Declined Order";
                var message = $"Driver {driverName} has declined your order and won't be able to proceed.";

                // Get the UserId from CargoOwner
                var cargoOwner = await _dbContext.CargoOwners.FirstOrDefaultAsync(co => co.Id == cargoOwnerId);
                if (cargoOwner?.UserId != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        cargoOwner.UserId,
                        title,
                        message,
                        NotificationType.OrderDeclined,
                        orderId,
                        "CargoOrders");
                }
            }, "DriverDeclined");
        }

        // Order started notification for cargo owner
        public async Task NotifyOrderStarted(string cargoOwnerId, string orderId, string driverName)
        {
            await SafeExecuteNotificationAsync(async () =>
            {
                var title = "Order Started";
                var message = $"Driver {driverName} has started your order and is heading to the pickup location.";

                // Get the UserId from CargoOwner
                var cargoOwner = await _dbContext.CargoOwners.FirstOrDefaultAsync(co => co.Id == cargoOwnerId);
                if (cargoOwner?.UserId != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        cargoOwner.UserId,
                        title,
                        message,
                        NotificationType.OrderStarted,
                        orderId,
                        "CargoOrders");
                }
            }, "OrderStarted");
        }

        // Order picked up notification for cargo owner
        public async Task NotifyOrderPickedUp(string cargoOwnerId, string orderId, string driverName)
        {
            await SafeExecuteNotificationAsync(async () =>
            {
                var title = "Order Picked Up";
                var message = $"Driver {driverName} has picked up your order and is now in transit.";

                // Get the UserId from CargoOwner
                var cargoOwner = await _dbContext.CargoOwners.FirstOrDefaultAsync(co => co.Id == cargoOwnerId);
                if (cargoOwner?.UserId != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        cargoOwner.UserId,
                        title,
                        message,
                        NotificationType.OrderPickedUp,
                        orderId,
                        "CargoOrders");
                }
            }, "OrderPickedUp");
        }

        // Order delivered notification for cargo owner
        public async Task NotifyOrderDelivered(string cargoOwnerId, string orderId, string driverName)
        {
            await SafeExecuteNotificationAsync(async () =>
            {
                var title = "Order Delivered";
                var message = $"Driver {driverName} has delivered your order successfully.";

                // Get the UserId from CargoOwner
                var cargoOwner = await _dbContext.CargoOwners.FirstOrDefaultAsync(co => co.Id == cargoOwnerId);
                if (cargoOwner?.UserId != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        cargoOwner.UserId,
                        title,
                        message,
                        NotificationType.OrderDelivered,
                        orderId,
                        "CargoOrders");
                }
            }, "OrderDelivered");
        }

        // Document uploaded notification for cargo owner
        public async Task NotifyDocumentUploaded(string cargoOwnerId, string orderId, string documentType)
        {
            await SafeExecuteNotificationAsync(async () =>
            {
                var title = "Document Uploaded";
                var message = $"A new {documentType} document has been uploaded for your order.";

                // Get the UserId from CargoOwner
                var cargoOwner = await _dbContext.CargoOwners.FirstOrDefaultAsync(co => co.Id == cargoOwnerId);
                if (cargoOwner?.UserId != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        cargoOwner.UserId,
                        title,
                        message,
                        NotificationType.DocumentUploaded,
                        orderId,
                        "CargoOrders");
                }
            }, "DocumentUploaded");
        }

        // Payment received notification for driver
        public async Task NotifyPaymentReceived(string driverId, string orderId, decimal amount)
        {
            await SafeExecuteNotificationAsync(async () =>
            {
                var title = "Payment Received";
                var message = $"You have received a payment of ${amount} for your delivery.";

                // Get the UserId from Driver
                var driver = await _dbContext.Drivers.FirstOrDefaultAsync(d => d.Id == driverId);
                if (driver?.UserId != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        driver.UserId,
                        title,
                        message,
                        NotificationType.PaymentReceived,
                        orderId,
                        "CargoOrders");
                }
            }, "PaymentReceived");
        }

        // Account approved notification for driver/truck owner
        public async Task NotifyAccountApproved(string userId, string entityType)
        {
            await SafeExecuteNotificationAsync(async () =>
            {
                var title = "Account Approved";
                var message = $"Your {entityType} account has been approved. You can now use all features.";

                await _notificationService.CreateNotificationAsync(
                    userId,
                    title,
                    message,
                    NotificationType.AccountApproved,
                    null,
                    entityType);
            }, "AccountApproved");
        }

        // New message notification
        public async Task NotifyNewMessage(string recipientId, string senderId, string orderTitle)
        {
            await SafeExecuteNotificationAsync(async () =>
            {
                var title = "New Message";
                var message = $"You have received a new message regarding order: {orderTitle}";

                await _notificationService.CreateNotificationAsync(
                    recipientId,
                    title,
                    message,
                    NotificationType.NewMessage,
                    senderId,
                    "ChatMessage");
            }, "NewMessage");
        }

        // Location update notification for cargo owner - push notification only
        public async Task NotifyLocationUpdated(string cargoOwnerId, string orderId, string currentLocation, DateTime estimatedArrival)
        {
            await SafeExecuteNotificationAsync(async () =>
            {
                var title = "Cargo Location Updated";
                var message = $"Your cargo is now at {currentLocation}. " +
                             $"Estimated arrival: {estimatedArrival.ToString("g")}";

                // Get the UserId from CargoOwner
                var cargoOwner = await _dbContext.CargoOwners.FirstOrDefaultAsync(co => co.Id == cargoOwnerId);
                if (cargoOwner?.UserId != null)
                {
                    // Only send push notification without creating database entry
                    await _notificationService.SendNotificationAsync(
                        cargoOwner.UserId,
                        title,
                        message,
                        new Dictionary<string, string> {
                            { "orderId", orderId },
                            { "type", "location_update" },
                            { "currentLocation", currentLocation },
                            { "estimatedArrival", estimatedArrival.ToString("o") }
                        }
                    );
                }
            }, "LocationUpdated");
        }

        // Notify cargo owner about payment reminder
        public async Task NotifyPaymentReminder(string cargoOwnerId, string orderId, string invoiceNumber, decimal amount, DateTime dueDate)
        {
            await SafeExecuteNotificationAsync(async () =>
            {
                var title = "Payment Reminder";
                var message = $"Invoice #{invoiceNumber} for ${amount} is due on {dueDate.ToShortDateString()}. Please submit payment to avoid late fees.";

                // Get the UserId from CargoOwner
                var cargoOwner = await _dbContext.CargoOwners.FirstOrDefaultAsync(co => co.Id == cargoOwnerId);
                if (cargoOwner?.UserId != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        cargoOwner.UserId,
                        title,
                        message,
                        NotificationType.PaymentReminder,
                        orderId,
                        "CargoOrders");
                }
            }, "PaymentReminder");
        }

        // Notify driver about upcoming pickup
        public async Task NotifyPickupReminder(string driverId, string orderId, string pickupLocation, DateTime pickupTime)
        {
            await SafeExecuteNotificationAsync(async () =>
            {
                var timeUntilPickup = pickupTime - DateTime.UtcNow;
                string timeMessage;

                if (timeUntilPickup.TotalHours < 1)
                    timeMessage = $"{(int)timeUntilPickup.TotalMinutes} minutes";
                else if (timeUntilPickup.TotalHours < 24)
                    timeMessage = $"{(int)timeUntilPickup.TotalHours} hours";
                else
                    timeMessage = $"{(int)timeUntilPickup.TotalDays} days";

                var title = "Upcoming Pickup Reminder";
                var message = $"You have a cargo pickup at {pickupLocation} in {timeMessage}.";

                // Get the UserId from Driver
                var driver = await _dbContext.Drivers.FirstOrDefaultAsync(d => d.Id == driverId);
                if (driver?.UserId != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        driver.UserId,
                        title,
                        message,
                        NotificationType.PickupReminder,
                        orderId,
                        "CargoOrders");
                }
            }, "PickupReminder");
        }

        // Notify cargo owner about potential delivery delay
        public async Task NotifyDeliveryDelay(string cargoOwnerId, string orderId, string reason, DateTime newEstimatedTime)
        {
            await SafeExecuteNotificationAsync(async () =>
            {
                var title = "Delivery Delay";
                var message = $"Your delivery is delayed due to {reason}. New estimated arrival: {newEstimatedTime.ToString("g")}";

                // Get the UserId from CargoOwner
                var cargoOwner = await _dbContext.CargoOwners.FirstOrDefaultAsync(co => co.Id == cargoOwnerId);
                if (cargoOwner?.UserId != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        cargoOwner.UserId,
                        title,
                        message,
                        NotificationType.DeliveryDelay,
                        orderId,
                        "CargoOrders");
                }
            }, "DeliveryDelay");
        }

        // Notify driver that cargo delivery has been confirmed and their job is complete
        public async Task NotifyDeliveryConfirmed(string driverId, string orderId, string pickupLocation, string deliveryLocation)
        {
            await SafeExecuteNotificationAsync(async () =>
            {
                var title = "Delivery Confirmed";
                var message = $"Your delivery from {pickupLocation} to {deliveryLocation} has been confirmed. Thank you for your service!";

                // Get the UserId from Driver
                var driver = await _dbContext.Drivers.FirstOrDefaultAsync(d => d.Id == driverId);
                if (driver?.UserId != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        driver.UserId,
                        title,
                        message,
                        NotificationType.DeliveryConfirmed,
                        orderId,
                        "CargoOrders");
                }
            }, "DeliveryConfirmed");
        }

        // Notify driver of a rating/review from the cargo owner
        public async Task NotifyDriverRated(string driverId, string orderId, int rating, string comment = null)
        {
            await SafeExecuteNotificationAsync(async () =>
            {
                var title = "New Rating Received";
                var message = rating >= 4
                    ? $"You received a {rating}-star rating for your recent delivery! {(string.IsNullOrEmpty(comment) ? "" : $"Comment: \"{comment}\"")}"
                    : $"You received a {rating}-star rating for your recent delivery. We encourage you to maintain our service standards.";

                // Get the UserId from Driver
                var driver = await _dbContext.Drivers.FirstOrDefaultAsync(d => d.Id == driverId);
                if (driver?.UserId != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        driver.UserId,
                        title,
                        message,
                        NotificationType.DriverRated,
                        orderId,
                        "CargoOrders");
                }
            }, "DriverRated");
        }

        // Notify driver of a payment released to their account (if applicable in your system)
        public async Task NotifyPaymentReleased(string driverId, string orderId, decimal amount, string transferId)
        {
            await SafeExecuteNotificationAsync(async () =>
            {
                var title = "Payment Released";
                var message = $"A payment of ${amount} has been released to your account for completed delivery.";

                // Get the UserId from Driver
                var driver = await _dbContext.Drivers.FirstOrDefaultAsync(d => d.Id == driverId);
                if (driver?.UserId != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        driver.UserId,
                        title,
                        message,
                        NotificationType.PaymentReleased,
                        orderId,
                        "CargoOrders");

                    // Also send as push notification for immediate awareness
                    await _notificationService.SendNotificationAsync(
                        driver.UserId,
                        title,
                        message,
                        new Dictionary<string, string> {
                            { "orderId", orderId },
                            { "type", "payment_released" },
                            { "amount", amount.ToString() },
                            { "transferId", transferId }
                        }
                    );
                }
            }, "PaymentReleased");
        }
        public async Task NotifyInvoiceOverdue(string cargoOwnerId, string orderId, string invoiceNumber, decimal amount, DateTime dueDate)
        {
            await SafeExecuteNotificationAsync(async () =>
            {
                var title = "Invoice Overdue";
                var message = $"Invoice #{invoiceNumber} for ${amount} was due on {dueDate.ToShortDateString()} and is now overdue. Please submit payment as soon as possible.";

                // Get the UserId from CargoOwner
                var cargoOwner = await _dbContext.CargoOwners.FirstOrDefaultAsync(co => co.Id == cargoOwnerId);
                if (cargoOwner?.UserId != null)
                {
                    // Send push notification
                    await _notificationService.SendNotificationAsync(
                        cargoOwner.UserId,
                        title,
                        message,
                        new Dictionary<string, string> {
                    { "orderId", orderId },
                    { "invoiceNumber", invoiceNumber },
                    { "type", "invoice_overdue" }
                        }
                    );

                    // Create database notification
                    await _notificationService.CreateNotificationAsync(
                        cargoOwner.UserId,
                        title,
                        message,
                        NotificationType.PaymentReminder, // You might add a specific NotificationType.InvoiceOverdue if preferred
                        orderId,
                        "CargoOrders");
                }
            }, "InvoiceOverdue");
        }
        /// <summary>
        /// Notify driver when their assigned order is cancelled
        /// </summary>
        public async Task NotifyOrderCancelled(
            string driverId,
            string orderId,
            string pickupLocation,
            string deliveryLocation,
            string cancellationReason)
        {
            try
            {
                var driver = await _dbContext.Drivers
                    .FirstOrDefaultAsync(d => d.Id == driverId);

                if (driver?.UserId == null)
                {
                    _logger.LogWarning("Driver not found or has no user account: {DriverId}", driverId);
                    return;
                }

                // Create database notification
                await _notificationService.CreateNotificationAsync(
                    driver.UserId,
                    "Order Cancelled",
                    $"Your assigned order from {pickupLocation} to {deliveryLocation} has been cancelled. Reason: {cancellationReason}",
                    NotificationType.OrderCancelled,
                    orderId,
                    "Order");

                // Send push notification
                await _notificationService.SendNotificationAsync(
                    driver.UserId,
                    "Order Cancelled",
                    $"Order from {pickupLocation} to {deliveryLocation} has been cancelled",
                    new Dictionary<string, string>
                    {
                        { "orderId", orderId },
                        { "type", "order_cancelled" },
                        { "driverId", driverId },
                        { "pickupLocation", pickupLocation },
                        { "deliveryLocation", deliveryLocation },
                        { "reason", cancellationReason }
                    });

                _logger.LogInformation("Order cancellation notification sent to driver {DriverId} for order {OrderId}",
                    driverId, orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order cancellation notification to driver {DriverId} for order {OrderId}",
                    driverId, orderId);
            }
        }

        /// <summary>
        /// Notify cargo owner when order cancellation is confirmed
        /// </summary>
        public async Task NotifyOrderCancellationConfirmed(
            string cargoOwnerId,
            string orderId,
            string pickupLocation,
            string deliveryLocation,
            decimal refundAmount,
            decimal penaltyAmount)
        {
            try
            {
                var cargoOwner = await _dbContext.CargoOwners
                    .FirstOrDefaultAsync(co => co.Id == cargoOwnerId);

                if (cargoOwner?.UserId == null)
                {
                    _logger.LogWarning("Cargo owner not found or has no user account: {CargoOwnerId}", cargoOwnerId);
                    return;
                }

                var message = penaltyAmount > 0
                    ? $"Your order from {pickupLocation} to {deliveryLocation} has been cancelled. Refund: ${refundAmount:F2} (after ${penaltyAmount:F2} penalty)"
                    : $"Your order from {pickupLocation} to {deliveryLocation} has been cancelled. Refund: ${refundAmount:F2}";

                // Create database notification
                await _notificationService.CreateNotificationAsync(
                    cargoOwner.UserId,
                    "Order Cancellation Confirmed",
                    message,
                    NotificationType.OrderCancelled,
                    orderId,
                    "Order");

                // Send push notification
                await _notificationService.SendNotificationAsync(
                    cargoOwner.UserId,
                    "Order Cancellation Confirmed",
                    $"Order cancellation processed. Refund: ${refundAmount:F2}",
                    new Dictionary<string, string>
                    {
                        { "orderId", orderId },
                        { "type", "cancellation_confirmed" },
                        { "cargoOwnerId", cargoOwnerId },
                        { "refundAmount", refundAmount.ToString("F2") },
                        { "penaltyAmount", penaltyAmount.ToString("F2") }
                    });

                _logger.LogInformation("Order cancellation confirmation sent to cargo owner {CargoOwnerId} for order {OrderId}",
                    cargoOwnerId, orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order cancellation confirmation to cargo owner {CargoOwnerId} for order {OrderId}",
                    cargoOwnerId, orderId);
            }
        }

        /// <summary>
        /// Notify when refund is processed
        /// </summary>
        public async Task NotifyRefundProcessed(
            string cargoOwnerId,
            string orderId,
            decimal refundAmount,
            string refundMethod)
        {
            try
            {
                var cargoOwner = await _dbContext.CargoOwners
                    .FirstOrDefaultAsync(co => co.Id == cargoOwnerId);

                if (cargoOwner?.UserId == null)
                {
                    _logger.LogWarning("Cargo owner not found or has no user account: {CargoOwnerId}", cargoOwnerId);
                    return;
                }

                var message = refundMethod.ToLower() switch
                {
                    "wallet" => $"Your refund of ${refundAmount:F2} has been added to your wallet",
                    "stripe" => $"Your refund of ${refundAmount:F2} has been processed to your original payment method",
                    "invoice" => "Your invoice has been voided - no payment required",
                    _ => $"Your refund of ${refundAmount:F2} is being processed"
                };

                // Create database notification
                await _notificationService.CreateNotificationAsync(
                    cargoOwner.UserId,
                    "Refund Processed",
                    message,
                    NotificationType.PaymentReceived,
                    orderId,
                    "Refund");

                // Send push notification
                await _notificationService.SendNotificationAsync(
                    cargoOwner.UserId,
                    "Refund Processed",
                    $"Refund of ${refundAmount:F2} processed",
                    new Dictionary<string, string>
                    {
                        { "orderId", orderId },
                        { "type", "refund_processed" },
                        { "cargoOwnerId", cargoOwnerId },
                        { "refundAmount", refundAmount.ToString("F2") },
                        { "refundMethod", refundMethod }
                    });

                _logger.LogInformation("Refund processed notification sent to cargo owner {CargoOwnerId} for order {OrderId}",
                    cargoOwnerId, orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending refund processed notification to cargo owner {CargoOwnerId} for order {OrderId}",
                    cargoOwnerId, orderId);
            }
        }

    }
}