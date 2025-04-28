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
        OrderCreated,
        BidSubmitted,
        BidAccepted,
        OrderAssigned,
        OrderStarted,
        OrderPickedUp,
        OrderInTransit,
        OrderDelivered,
        DocumentUploaded,
        PaymentReceived,
        AccountApproved,
        NewMessage
    }
}