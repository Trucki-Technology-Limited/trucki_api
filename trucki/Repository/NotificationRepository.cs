using AutoMapper;
using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly TruckiDBContext _context;
        private readonly IMapper _mapper;
        
        public NotificationRepository(TruckiDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        
        public async Task<ApiResponseModel<PagedResponse<NotificationResponseModel>>> GetNotificationsAsync(
            string userId, GetNotificationsQueryDto query)
        {
            try
            {
                // Start with the base query filtering by userId
                var notificationsQuery = _context.Notifications
                    .Where(n => n.UserId == userId);
                
                // Apply IsRead filter if provided
                if (query.IsRead.HasValue)
                {
                    notificationsQuery = notificationsQuery.Where(n => n.IsRead == query.IsRead.Value);
                }
                
                // Get total count
                var totalCount = await notificationsQuery.CountAsync();
                
                // Apply pagination and ordering
                var notifications = await notificationsQuery
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();
                
                // Map to response model
                var notificationResponses = _mapper.Map<List<NotificationResponseModel>>(notifications);
                
                // Create paged response
                var pagedResponse = new PagedResponse<NotificationResponseModel>
                {
                    Data = notificationResponses,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
                };
                
                return ApiResponseModel<PagedResponse<NotificationResponseModel>>.Success(
                    "Notifications retrieved successfully",
                    pagedResponse,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<PagedResponse<NotificationResponseModel>>.Fail(
                    $"Error retrieving notifications: {ex.Message}",
                    500);
            }
        }
        
        public async Task<ApiResponseModel<NotificationCountResponseModel>> GetNotificationCountAsync(string userId)
        {
            try
            {
                var totalCount = await _context.Notifications
                    .CountAsync(n => n.UserId == userId);
                
                var unreadCount = await _context.Notifications
                    .CountAsync(n => n.UserId == userId && !n.IsRead);
                
                var countResponse = new NotificationCountResponseModel
                {
                    TotalCount = totalCount,
                    UnreadCount = unreadCount
                };
                
                return ApiResponseModel<NotificationCountResponseModel>.Success(
                    "Notification counts retrieved successfully",
                    countResponse,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<NotificationCountResponseModel>.Fail(
                    $"Error retrieving notification counts: {ex.Message}",
                    500);
            }
        }
        
        public async Task<ApiResponseModel<bool>> MarkAsReadAsync(string userId, string notificationId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
                
                if (notification == null)
                {
                    return ApiResponseModel<bool>.Fail(
                        "Notification not found or doesn't belong to the user",
                        404);
                }
                
                notification.IsRead = true;
                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();
                
                return ApiResponseModel<bool>.Success(
                    "Notification marked as read",
                    true,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail(
                    $"Error marking notification as read: {ex.Message}",
                    500);
            }
        }
        
        public async Task<ApiResponseModel<bool>> MarkMultipleAsReadAsync(string userId, List<string> notificationIds)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => notificationIds.Contains(n.Id) && n.UserId == userId)
                    .ToListAsync();
                
                if (!notifications.Any())
                {
                    return ApiResponseModel<bool>.Fail(
                        "No valid notifications found",
                        404);
                }
                
                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                }
                
                _context.Notifications.UpdateRange(notifications);
                await _context.SaveChangesAsync();
                
                return ApiResponseModel<bool>.Success(
                    $"{notifications.Count} notifications marked as read",
                    true,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail(
                    $"Error marking notifications as read: {ex.Message}",
                    500);
            }
        }
        
        public async Task<ApiResponseModel<bool>> MarkAllAsReadAsync(string userId)
        {
            try
            {
                var unreadNotifications = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ToListAsync();
                
                if (!unreadNotifications.Any())
                {
                    return ApiResponseModel<bool>.Success(
                        "No unread notifications found",
                        true,
                        200);
                }
                
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                }
                
                _context.Notifications.UpdateRange(unreadNotifications);
                await _context.SaveChangesAsync();
                
                return ApiResponseModel<bool>.Success(
                    $"{unreadNotifications.Count} notifications marked as read",
                    true,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail(
                    $"Error marking all notifications as read: {ex.Message}",
                    500);
            }
        }
        
        public async Task<ApiResponseModel<bool>> DeleteNotificationAsync(string userId, string notificationId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
                
                if (notification == null)
                {
                    return ApiResponseModel<bool>.Fail(
                        "Notification not found or doesn't belong to the user",
                        404);
                }
                
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
                
                return ApiResponseModel<bool>.Success(
                    "Notification deleted successfully",
                    true,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail(
                    $"Error deleting notification: {ex.Message}",
                    500);
            }
        }
        
        public async Task<ApiResponseModel<bool>> DeleteMultipleNotificationsAsync(string userId, List<string> notificationIds)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => notificationIds.Contains(n.Id) && n.UserId == userId)
                    .ToListAsync();
                
                if (!notifications.Any())
                {
                    return ApiResponseModel<bool>.Fail(
                        "No valid notifications found",
                        404);
                }
                
                _context.Notifications.RemoveRange(notifications);
                await _context.SaveChangesAsync();
                
                return ApiResponseModel<bool>.Success(
                    $"{notifications.Count} notifications deleted successfully",
                    true,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail(
                    $"Error deleting notifications: {ex.Message}",
                    500);
            }
        }
        
        public async Task<ApiResponseModel<bool>> DeleteAllNotificationsAsync(string userId)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .ToListAsync();
                
                if (!notifications.Any())
                {
                    return ApiResponseModel<bool>.Success(
                        "No notifications found to delete",
                        true,
                        200);
                }
                
                _context.Notifications.RemoveRange(notifications);
                await _context.SaveChangesAsync();
                
                return ApiResponseModel<bool>.Success(
                    $"{notifications.Count} notifications deleted successfully",
                    true,
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail(
                    $"Error deleting all notifications: {ex.Message}",
                    500);
            }
        }
        
        public async Task<ApiResponseModel<bool>> CreateNotificationAsync(
            string userId, 
            string title, 
            string message, 
            NotificationType type, 
            string relatedEntityId = null, 
            string relatedEntityType = null)
        {
            try
            {
                var notification = new DatabaseNotification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    RelatedEntityId = relatedEntityId,
                    RelatedEntityType = relatedEntityType,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                
                return ApiResponseModel<bool>.Success(
                    "Notification created successfully",
                    true,
                    201);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail(
                    $"Error creating notification: {ex.Message}",
                    500);
            }
        }
        
        public async Task<ApiResponseModel<bool>> CreateNotificationsForMultipleUsersAsync(
            List<string> userIds, 
            string title, 
            string message, 
            NotificationType type, 
            string relatedEntityId = null, 
            string relatedEntityType = null)
        {
            try
            {
                var notifications = new List<DatabaseNotification>();
                
                foreach (var userId in userIds)
                {
                    notifications.Add(new DatabaseNotification
                    {
                        UserId = userId,
                        Title = title,
                        Message = message,
                        Type = type,
                        RelatedEntityId = relatedEntityId,
                        RelatedEntityType = relatedEntityType,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                
                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();
                
                return ApiResponseModel<bool>.Success(
                    $"{notifications.Count} notifications created successfully",
                    true,
                    201);
            }
            catch (Exception ex)
            {
                return ApiResponseModel<bool>.Fail(
                    $"Error creating notifications: {ex.Message}",
                    500);
            }
        }
    }
}