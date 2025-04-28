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

        public NotificationEventService(INotificationService notificationService, TruckiDBContext dbContext)
        {
            _notificationService = notificationService;
            _dbContext = dbContext;
        }

        // Order created notification for cargo owner
        public async Task NotifyOrderCreated(string cargoOwnerId, string orderId, string pickupLocation, string deliveryLocation)
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
        }

        // Order open for bidding notification for all available drivers
        public async Task NotifyOrderOpenForBidding(string orderId, string pickupLocation, string deliveryLocation)
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
        }

        // Bid submitted notification for cargo owner
        public async Task NotifyBidSubmitted(string cargoOwnerId, string orderId, string driverName, decimal amount)
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
        }

        // Bid accepted notification for driver
        public async Task NotifyBidAccepted(string driverId, string orderId, string pickupLocation, string deliveryLocation)
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
        }

        // Driver acknowledged notification for cargo owner
        public async Task NotifyDriverAcknowledged(string cargoOwnerId, string orderId, string driverName)
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
        }

        // Order started notification for cargo owner
        public async Task NotifyOrderStarted(string cargoOwnerId, string orderId, string driverName)
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
        }

        // Order picked up notification for cargo owner
        public async Task NotifyOrderPickedUp(string cargoOwnerId, string orderId, string driverName)
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
        }

        // Order delivered notification for cargo owner
        public async Task NotifyOrderDelivered(string cargoOwnerId, string orderId, string driverName)
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
        }

        // Document uploaded notification for cargo owner
        public async Task NotifyDocumentUploaded(string cargoOwnerId, string orderId, string documentType)
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
        }

        // Payment received notification for driver
        public async Task NotifyPaymentReceived(string driverId, string orderId, decimal amount)
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
        }

        // Account approved notification for driver/truck owner
        public async Task NotifyAccountApproved(string userId, string entityType)
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
        }

        // New message notification
        public async Task NotifyNewMessage(string recipientId, string senderId, string orderTitle)
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
        }
    }
}