using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace trucki.Entities
{
    public class DatabaseNotification : BaseClass
    {
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public NotificationType Type { get; set; }

        public string RelatedEntityId { get; set; }

        public string RelatedEntityType { get; set; }

        public bool IsRead { get; set; } = false;
    }

    public enum NotificationType
    {
        // Existing types
        OrderCreated,
        OrderAssigned,
        OrderStarted,
        OrderPickedUp,
        OrderDelivered,
        OrderDeclined,
        BidSubmitted,
        BidAccepted,
        DocumentUploaded,
        PaymentReceived,
        AccountApproved,
        NewMessage,

        // New types
        LocationUpdated,           // For location updates (not stored in DB)
        PaymentReminder,           // For payment due reminders
        PickupReminder,            // For upcoming pickup reminders
        DeliveryDelay,             // For delivery delay notifications
        DeliveryConfirmed,         // For confirmed deliveries
        DriverRated,               // For driver ratings
        PaymentReleased,          // For payment releases to drivers
        OrderCancelled,           // When an order is cancelled
        CancellationApproved,     // When cancellation is approved (if approval needed)
        RefundProcessed,          // When refund is completed
        CancellationPenalty,      // When a penalty is applied
        InvoiceVoided
    }
}