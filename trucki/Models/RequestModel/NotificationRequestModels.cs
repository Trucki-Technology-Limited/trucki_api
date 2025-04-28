using System.ComponentModel.DataAnnotations;

namespace trucki.Models.RequestModel
{
    public class MarkNotificationAsReadDto
    {
        [Required]
        public string NotificationId { get; set; }
    }
    
    public class MarkMultipleNotificationsAsReadDto
    {
        [Required]
        public List<string> NotificationIds { get; set; }
    }
    
    public class DeleteNotificationDto
    {
        [Required]
        public string NotificationId { get; set; }
    }
    
    public class DeleteMultipleNotificationsDto
    {
        [Required]
        public List<string> NotificationIds { get; set; }
    }
    
    public class GetNotificationsQueryDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool? IsRead { get; set; } = null; // Filter by read status if provided
    }
}