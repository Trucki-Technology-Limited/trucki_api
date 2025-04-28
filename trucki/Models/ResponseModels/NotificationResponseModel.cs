using System;
using trucki.Entities;

namespace trucki.Models.ResponseModels
{
    public class NotificationResponseModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public string RelatedEntityId { get; set; }
        public string RelatedEntityType { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    public class NotificationCountResponseModel
    {
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
    }
}