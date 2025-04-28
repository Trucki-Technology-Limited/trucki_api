using trucki.Entities;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IRepository
{
    public interface INotificationRepository
    {
        // Get notifications for a user
        Task<ApiResponseModel<PagedResponse<NotificationResponseModel>>> GetNotificationsAsync(
            string userId, GetNotificationsQueryDto query);
        
        // Get notification count for a user
        Task<ApiResponseModel<NotificationCountResponseModel>> GetNotificationCountAsync(string userId);
        
        // Mark a notification as read
        Task<ApiResponseModel<bool>> MarkAsReadAsync(string userId, string notificationId);
        
        // Mark multiple notifications as read
        Task<ApiResponseModel<bool>> MarkMultipleAsReadAsync(string userId, List<string> notificationIds);
        
        // Mark all notifications as read
        Task<ApiResponseModel<bool>> MarkAllAsReadAsync(string userId);
        
        // Delete a notification
        Task<ApiResponseModel<bool>> DeleteNotificationAsync(string userId, string notificationId);
        
        // Delete multiple notifications
        Task<ApiResponseModel<bool>> DeleteMultipleNotificationsAsync(string userId, List<string> notificationIds);
        
        // Delete all notifications
        Task<ApiResponseModel<bool>> DeleteAllNotificationsAsync(string userId);
        
        // Create a notification
        Task<ApiResponseModel<bool>> CreateNotificationAsync(
            string userId, 
            string title, 
            string message, 
            NotificationType type, 
            string relatedEntityId = null, 
            string relatedEntityType = null);
        
        // Create notifications for multiple users
        Task<ApiResponseModel<bool>> CreateNotificationsForMultipleUsersAsync(
            List<string> userIds, 
            string title, 
            string message, 
            NotificationType type, 
            string relatedEntityId = null, 
            string relatedEntityType = null);
    }
}