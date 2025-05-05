// trucki/Services/FirebaseNotificationService.cs
using System.Text.RegularExpressions;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services;

public class NotificationService : INotificationService
{
    private readonly UserManager<User> _userManager;
    private readonly TruckiDBContext _dbContext;
    private readonly ILogger<NotificationService> _logger;
    private readonly INotificationRepository _notificationRepository;


    public NotificationService(TruckiDBContext dbContext, ILogger<NotificationService> logger, UserManager<User> userManager, INotificationRepository notificationRepository)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _logger = logger;
        _notificationRepository = notificationRepository;


        // Initialize Firebase if not already initialized
        try
        {
            // Try to get the default instance — if it doesn't exist, it will throw.
            var _ = FirebaseApp.DefaultInstance;
        }
        catch (InvalidOperationException)
        {
            // Create the app only if it hasn’t been created yet
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile("trucki-c0df5-firebase-adminsdk-fbsvc-14d406ba99.json")
            });

            _logger.LogInformation("Firebase initialized successfully");
        }

    }

    public async Task SendNotificationAsync(string userId, string title, string body, Dictionary<string, string> data = null)
    {
        try
        {
            // Get all devices for this user
            var devices = await _dbContext.DeviceTokens
                .Where(d => d.UserId == userId)
                .ToListAsync();

            if (!devices.Any())
            {
                _logger.LogWarning($"No device tokens found for user {userId}");
                return;
            }

            var message = new MulticastMessage
            {
                Tokens = devices.Select(d => d.Token).ToList(),
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data ?? new Dictionary<string, string>()
            };

            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
            _logger.LogInformation($"Successfully sent notification to {response.SuccessCount} devices for user {userId}");

            // Handle failures if any
            if (response.FailureCount > 0)
            {
                var failedTokens = new List<string>();

                for (var i = 0; i < response.Responses.Count; i++)
                {
                    if (!response.Responses[i].IsSuccess)
                    {
                        failedTokens.Add(devices[i].Token);
                        _logger.LogWarning($"Failed to send notification to token: {devices[i].Token}, reason: {response.Responses[i].Exception.Message}");
                    }
                }

                // Remove invalid tokens
                if (failedTokens.Any())
                {
                    var tokensToRemove = await _dbContext.DeviceTokens
                        .Where(d => failedTokens.Contains(d.Token))
                        .ToListAsync();

                    _dbContext.DeviceTokens.RemoveRange(tokensToRemove);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending notification: {ex.Message}");
            throw;
        }
    }

    public async Task SendNotificationToTopicAsync(string topic, string title, string body, Dictionary<string, string> data = null)
    {
        try
        {
            var message = new Message
            {
                Topic = topic,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data ?? new Dictionary<string, string>()
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation($"Successfully sent notification to topic {topic}, message ID: {response}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending notification to topic: {ex.Message}");
            throw;
        }
    }

    public async Task SubscribeToTopicAsync(string token, string topic)
    {
        try
        {
            // Sanitize the topic name to conform with FCM rules
            var sanitizedTopic = SanitizeTopicName(topic);

            var response = await FirebaseMessaging.DefaultInstance.SubscribeToTopicAsync(
                new List<string> { token }, sanitizedTopic);

            _logger.LogInformation($"Successfully subscribed token to topic '{sanitizedTopic}'. Success count: {response.SuccessCount}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error subscribing to topic '{topic}': {ex.Message}");
            throw;
        }
    }

    private string SanitizeTopicName(string topic)
    {
        // Converts to lowercase, removes invalid characters, and replaces spaces with underscores
        return Regex.Replace(topic.ToLowerInvariant(), @"[^a-z0-9_-]", "_");
    }


    public async Task UnsubscribeFromTopicAsync(string token, string topic)
    {
        try
        {
            var response = await FirebaseMessaging.DefaultInstance.UnsubscribeFromTopicAsync(
                new List<string> { token }, topic);

            _logger.LogInformation($"Successfully unsubscribed token from topic {topic}. Success count: {response.SuccessCount}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error unsubscribing from topic: {ex.Message}");
            throw;
        }
    }

    // trucki/Services/FirebaseNotificationService.cs
    public async Task SaveDeviceTokenAsync(string userId, string token, string deviceType)
    {
        try
        {
            // Check if token already exists
            var existingToken = await _dbContext.DeviceTokens
                .FirstOrDefaultAsync(d => d.Token == token);

            if (existingToken != null)
            {
                // Update if user ID or device type changed
                if (existingToken.UserId != userId || existingToken.DeviceType != deviceType)
                {
                    existingToken.UserId = userId;
                    existingToken.DeviceType = deviceType;
                    existingToken.UpdatedAt = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();
                }
                return;
            }

            // Create new token
            var deviceToken = new DeviceToken
            {
                UserId = userId,
                Token = token,
                DeviceType = deviceType,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.DeviceTokens.Add(deviceToken);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Device token saved for user {userId}");

            // Subscribe to user roles as topics
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    await SubscribeToTopicAsync(token, role.ToLower());
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving device token: {ex.Message}");
            throw;
        }

    }
    public async Task<ApiResponseModel<PagedResponse<NotificationResponseModel>>> GetNotificationsAsync(
    string userId, GetNotificationsQueryDto query)
    {
        return await _notificationRepository.GetNotificationsAsync(userId, query);
    }

    public async Task<ApiResponseModel<NotificationCountResponseModel>> GetNotificationCountAsync(string userId)
    {
        return await _notificationRepository.GetNotificationCountAsync(userId);
    }

    public async Task<ApiResponseModel<bool>> MarkAsReadAsync(string userId, string notificationId)
    {
        return await _notificationRepository.MarkAsReadAsync(userId, notificationId);
    }

    public async Task<ApiResponseModel<bool>> MarkMultipleAsReadAsync(string userId, List<string> notificationIds)
    {
        return await _notificationRepository.MarkMultipleAsReadAsync(userId, notificationIds);
    }

    public async Task<ApiResponseModel<bool>> MarkAllAsReadAsync(string userId)
    {
        return await _notificationRepository.MarkAllAsReadAsync(userId);
    }

    public async Task<ApiResponseModel<bool>> DeleteNotificationAsync(string userId, string notificationId)
    {
        return await _notificationRepository.DeleteNotificationAsync(userId, notificationId);
    }

    public async Task<ApiResponseModel<bool>> DeleteMultipleNotificationsAsync(string userId, List<string> notificationIds)
    {
        return await _notificationRepository.DeleteMultipleNotificationsAsync(userId, notificationIds);
    }

    public async Task<ApiResponseModel<bool>> DeleteAllNotificationsAsync(string userId)
    {
        return await _notificationRepository.DeleteAllNotificationsAsync(userId);
    }

    public async Task<ApiResponseModel<bool>> CreateNotificationAsync(
        string userId,
        string title,
        string message,
        NotificationType type,
        string relatedEntityId = null,
        string relatedEntityType = null)
    {
        return await _notificationRepository.CreateNotificationAsync(
            userId, title, message, type, relatedEntityId, relatedEntityType);
    }

    public async Task<ApiResponseModel<bool>> CreateNotificationsForMultipleUsersAsync(
        List<string> userIds,
        string title,
        string message,
        NotificationType type,
        string relatedEntityId = null,
        string relatedEntityType = null)
    {
        return await _notificationRepository.CreateNotificationsForMultipleUsersAsync(
            userIds, title, message, type, relatedEntityId, relatedEntityType);
    }
}