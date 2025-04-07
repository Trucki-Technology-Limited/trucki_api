// trucki/Services/FirebaseNotificationService.cs
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IServices;

namespace trucki.Services;

public class FirebaseNotificationService : INotificationService
{
    private readonly UserManager<User> _userManager;
    private readonly TruckiDBContext _dbContext;
    private readonly ILogger<FirebaseNotificationService> _logger;

    public FirebaseNotificationService(TruckiDBContext dbContext, ILogger<FirebaseNotificationService> logger, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _logger = logger;

        // Initialize Firebase if not already initialized
        if (FirebaseApp.DefaultInstance == null)
        {
            try
            {
                var app = FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile("trucki-c0df5-firebase-adminsdk-fbsvc-14d406ba99.json")
                });
                _logger.LogInformation("Firebase initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error initializing Firebase: {ex.Message}");
                throw;
            }
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
            var response = await FirebaseMessaging.DefaultInstance.SubscribeToTopicAsync(
                new List<string> { token }, topic);

            _logger.LogInformation($"Successfully subscribed token to topic {topic}. Success count: {response.SuccessCount}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error subscribing to topic: {ex.Message}");
            throw;
        }
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
}