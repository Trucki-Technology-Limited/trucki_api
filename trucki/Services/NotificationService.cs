// trucki/Services/NotificationService.cs
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
    private readonly IEmailService _emailService;


    public NotificationService(TruckiDBContext dbContext, ILogger<NotificationService> logger, UserManager<User> userManager, INotificationRepository notificationRepository, IEmailService emailService)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _logger = logger;
        _notificationRepository = notificationRepository;
        _emailService = emailService;
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

            // Use the retry mechanism for sending the notification
            var response = await RetryFirebaseOperationAsync(
                () => FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message),
                $"send notification to user {userId}");

            if (response == null)
            {
                _logger.LogWarning($"Failed to send notification to user {userId} after multiple attempts");
                return;
            }

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
            // Log but don't throw exceptions from notification failures
            _logger.LogError($"Error sending notification to user {userId}: {ex.Message}");
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

            // Use the retry mechanism for sending the topic notification
            var response = await RetryFirebaseOperationAsync(
                () => FirebaseMessaging.DefaultInstance.SendAsync(message),
                $"send notification to topic {topic}");

            if (response == null)
            {
                _logger.LogWarning($"Failed to send notification to topic {topic} after multiple attempts");
                return;
            }

            _logger.LogInformation($"Successfully sent notification to topic {topic}, message ID: {response}");
        }
        catch (Exception ex)
        {
            // Log but don't throw
            _logger.LogError($"Error sending notification to topic {topic}: {ex.Message}");
        }
    }

    public async Task SubscribeToTopicAsync(string token, string topic)
    {
        try
        {
            // Sanitize the topic name to conform with FCM rules
            var sanitizedTopic = SanitizeTopicName(topic);

            // Use the retry mechanism for subscribing to a topic
            var response = await RetryFirebaseOperationAsync(
                () => FirebaseMessaging.DefaultInstance.SubscribeToTopicAsync(
                    new List<string> { token }, sanitizedTopic),
                $"subscribe token to topic {sanitizedTopic}");

            if (response == null)
            {
                _logger.LogWarning($"Failed to subscribe token to topic {sanitizedTopic} after multiple attempts");
                return;
            }

            _logger.LogInformation($"Successfully subscribed token to topic '{sanitizedTopic}'. Success count: {response.SuccessCount}");
        }
        catch (Exception ex)
        {
            // Log but don't throw
            _logger.LogError($"Error subscribing to topic '{topic}': {ex.Message}");
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
            // Use the retry mechanism for unsubscribing from a topic
            var response = await RetryFirebaseOperationAsync(
                () => FirebaseMessaging.DefaultInstance.UnsubscribeFromTopicAsync(
                    new List<string> { token }, topic),
                $"unsubscribe token from topic {topic}");

            if (response == null)
            {
                _logger.LogWarning($"Failed to unsubscribe token from topic {topic} after multiple attempts");
                return;
            }

            _logger.LogInformation($"Successfully unsubscribed token from topic {topic}. Success count: {response.SuccessCount}");
        }
        catch (Exception ex)
        {
            // Log but don't throw
            _logger.LogError($"Error unsubscribing from topic {topic}: {ex.Message}");
        }
    }

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


            try
            {
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
                _logger.LogWarning($"Failed to subscribe user {userId} to role topics: {ex.Message}");
                // Continue execution - don't throw
            }

        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving device token for user {userId}: {ex.Message}");
            // Rethrow this specific exception since it's a core functionality
            throw;
        }
    }

    // Safe retry mechanism for Firebase operations
    private async Task<T> RetryFirebaseOperationAsync<T>(Func<Task<T>> operation, string operationName, int maxRetries = 3)
    {


        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Attempt {attempt} failed for {operationName}: {ex.Message}");

                if (attempt == maxRetries)
                {
                    _logger.LogError($"All {maxRetries} attempts failed for {operationName}");
                    return default;
                }

                // Exponential backoff: wait longer between each retry
                await Task.Delay(100 * (int)Math.Pow(2, attempt - 1));


            }
        }

        return default;
    }

    // The repository methods remain unchanged
    public async Task<ApiResponseModel<PagedResponse<NotificationResponseModel>>> GetNotificationsAsync(string userId, GetNotificationsQueryDto query)
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

    public async Task<ApiResponseModel<AdminNotificationResponseModel>> SendAdminNotificationAsync(AdminNotificationRequestModel request)
    {
        try
        {
            var targetUsers = await GetTargetUsersAsync(request.TargetType, request.UserType, request.UserIds);
            var response = new AdminNotificationResponseModel
            {
                TotalTargetUsers = targetUsers.Count
            };

            if (!targetUsers.Any())
            {
                response.Errors.Add("No users found matching the target criteria");
                return ApiResponseModel<AdminNotificationResponseModel>.Success("No users to notify", response, 200);
            }

            // Send push notifications
            if (request.Channel == NotificationChannelType.PushOnly || request.Channel == NotificationChannelType.Both)
            {
                foreach (var user in targetUsers)
                {
                    try
                    {
                        await SendNotificationAsync(user.Id, request.Title, request.Message, request.Data);
                        await CreateNotificationAsync(user.Id, request.Title, request.Message, NotificationType.AccountUpdate);
                        response.SuccessfulPushNotifications++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to send push notification to user {user.Id}: {ex.Message}");
                        response.FailedPushNotifications++;
                        response.Errors.Add($"Push notification failed for user {user.Email}: {ex.Message}");
                    }
                }
            }

            // Send emails
            if (request.Channel == NotificationChannelType.EmailOnly || request.Channel == NotificationChannelType.Both)
            {
                foreach (var user in targetUsers)
                {
                    try
                    {
                        await _emailService.SendGenericEmailAsync(
                            user.Email,
                            request.Title,
                            $"<h2>{request.Title}</h2><p>{request.Message}</p><p>Best regards,<br/>Trucki Team</p>");
                        response.SuccessfulEmails++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to send email to user {user.Email}: {ex.Message}");
                        response.FailedEmails++;
                        response.Errors.Add($"Email failed for user {user.Email}: {ex.Message}");
                    }
                }
            }

            return ApiResponseModel<AdminNotificationResponseModel>.Success("Notifications sent successfully", response, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in SendAdminNotificationAsync: {ex.Message}");
            return ApiResponseModel<AdminNotificationResponseModel>.Fail($"Failed to send notifications: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponseModel<AdminNotificationResponseModel>> SendAdminEmailNotificationAsync(AdminEmailNotificationRequestModel request)
    {
        try
        {
            var targetUsers = await GetTargetUsersAsync(request.TargetType, request.UserType, request.UserIds);
            var response = new AdminNotificationResponseModel
            {
                TotalTargetUsers = targetUsers.Count
            };

            if (!targetUsers.Any())
            {
                response.Errors.Add("No users found matching the target criteria");
                return ApiResponseModel<AdminNotificationResponseModel>.Success("No users to notify", response, 200);
            }

            foreach (var user in targetUsers)
            {
                try
                {
                    await _emailService.SendGenericEmailAsync(user.Email, request.Subject, request.HtmlBody);
                    response.SuccessfulEmails++;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to send email to user {user.Email}: {ex.Message}");
                    response.FailedEmails++;
                    response.Errors.Add($"Email failed for user {user.Email}: {ex.Message}");
                }
            }

            return ApiResponseModel<AdminNotificationResponseModel>.Success("Emails sent successfully", response, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in SendAdminEmailNotificationAsync: {ex.Message}");
            return ApiResponseModel<AdminNotificationResponseModel>.Fail($"Failed to send emails: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponseModel<List<UserTypeOptionResponseModel>>> GetUserTypeOptionsAsync()
    {
        try
        {
            var userTypes = new List<UserTypeOptionResponseModel>();

            // Get counts for each user type
            var driverCount = await _dbContext.Drivers.CountAsync();
            var cargoOwnerCount = await _dbContext.CargoOwners.CountAsync();
            var truckOwnerCount = await _dbContext.TruckOwners.CountAsync();
            var managerCount = await _dbContext.Managers.CountAsync();

            userTypes.Add(new UserTypeOptionResponseModel
            {
                UserType = "driver",
                DisplayName = "Drivers",
                UserCount = driverCount
            });

            userTypes.Add(new UserTypeOptionResponseModel
            {
                UserType = "cargoowner",
                DisplayName = "Cargo Owners",
                UserCount = cargoOwnerCount
            });

            userTypes.Add(new UserTypeOptionResponseModel
            {
                UserType = "truckowner",
                DisplayName = "Truck Owners",
                UserCount = truckOwnerCount
            });

            userTypes.Add(new UserTypeOptionResponseModel
            {
                UserType = "manager",
                DisplayName = "Managers",
                UserCount = managerCount
            });

            return ApiResponseModel<List<UserTypeOptionResponseModel>>.Success("User types retrieved successfully", userTypes, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in GetUserTypeOptionsAsync: {ex.Message}");
            return ApiResponseModel<List<UserTypeOptionResponseModel>>.Fail($"Failed to get user types: {ex.Message}", 500);
        }
    }

    private async Task<List<User>> GetTargetUsersAsync(NotificationTargetType targetType, string userType = null, List<string> userIds = null)
    {
        switch (targetType)
        {
            case NotificationTargetType.AllUsers:
                return await _dbContext.Users.Where(u => u.IsActive).ToListAsync();

            case NotificationTargetType.SpecificUsers:
                if (userIds == null || !userIds.Any())
                    return new List<User>();
                return await _dbContext.Users.Where(u => userIds.Contains(u.Id) && u.IsActive).ToListAsync();

            case NotificationTargetType.UserType:
                if (string.IsNullOrEmpty(userType))
                    return new List<User>();

                return userType.ToLower() switch
                {
                    "driver" => await _dbContext.Drivers
                        .Include(d => d.User)
                        .Where(d => d.User != null && d.User.IsActive)
                        .Select(d => d.User)
                        .ToListAsync(),

                    "cargoowner" => await _dbContext.CargoOwners
                        .Include(c => c.User)
                        .Where(c => c.User != null && c.User.IsActive)
                        .Select(c => c.User)
                        .ToListAsync(),

                    "truckowner" => await _dbContext.TruckOwners
                        .Include(t => t.User)
                        .Where(t => t.User != null && t.User.IsActive)
                        .Select(t => t.User)
                        .ToListAsync(),

                    "manager" => await _dbContext.Managers
                        .Include(m => m.User)
                        .Where(m => m.User != null && m.User.IsActive)
                        .Select(m => m.User)
                        .ToListAsync(),

                    _ => new List<User>()
                };

            default:
                return new List<User>();
        }
    }

    public async Task<ApiResponseModel<PagedResponse<AdminUserSearchResponseModel>>> SearchUsersForNotificationAsync(AdminUserSearchRequestModel request)
    {
        try
        {
            var query = _dbContext.Users.AsQueryable();

            // Filter by active status if specified
            if (request.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == request.IsActive.Value);
            }

            // Apply search query (search in email, first name, last name)
            if (!string.IsNullOrWhiteSpace(request.SearchQuery))
            {
                var searchTerm = request.SearchQuery.ToLower().Trim();
                query = query.Where(u =>
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.firstName.ToLower().Contains(searchTerm) ||
                    u.lastName.ToLower().Contains(searchTerm));
            }

            // Get the filtered users with their related entities
            var usersQuery = query.Select(u => new
            {
                User = u,
                Driver = _dbContext.Drivers.FirstOrDefault(d => d.UserId == u.Id),
                CargoOwner = _dbContext.CargoOwners.FirstOrDefault(c => c.UserId == u.Id),
                TruckOwner = _dbContext.TruckOwners.FirstOrDefault(t => t.UserId == u.Id),
                Manager = _dbContext.Managers.FirstOrDefault(m => m.UserId == u.Id)
            });

            // Filter by user type if specified
            if (!string.IsNullOrWhiteSpace(request.UserType))
            {
                switch (request.UserType.ToLower())
                {
                    case "driver":
                        usersQuery = usersQuery.Where(x => x.Driver != null);
                        break;
                    case "cargoowner":
                        usersQuery = usersQuery.Where(x => x.CargoOwner != null);
                        break;
                    case "truckowner":
                        usersQuery = usersQuery.Where(x => x.TruckOwner != null);
                        break;
                    case "manager":
                        usersQuery = usersQuery.Where(x => x.Manager != null);
                        break;
                }
            }

            // Get total count before pagination
            var totalCount = await usersQuery.CountAsync();

            // Apply pagination
            var users = await usersQuery
                .OrderBy(x => x.User.firstName)
                .ThenBy(x => x.User.lastName)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            // Map to response model
            var userResponseModels = users.Select(x => new AdminUserSearchResponseModel
            {
                UserId = x.User.Id,
                Email = x.User.Email ?? "",
                FirstName = x.User.firstName ?? "",
                LastName = x.User.lastName ?? "",
                IsActive = x.User.IsActive,
                CreatedAt = x.User.CreatedAt,
                UserType = GetUserType(x),
                UserTypeDisplay = GetUserTypeDisplay(x),
                Phone = GetUserPhone(x),
                CompanyName = GetCompanyName(x)
            }).ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            var pagedResponse = new PagedResponse<AdminUserSearchResponseModel>
            {
                Data = userResponseModels,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            return ApiResponseModel<PagedResponse<AdminUserSearchResponseModel>>.Success(
                "Users retrieved successfully", pagedResponse, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in SearchUsersForNotificationAsync: {ex.Message}");
            return ApiResponseModel<PagedResponse<AdminUserSearchResponseModel>>.Fail(
                $"Failed to search users: {ex.Message}", 500);
        }
    }

    private string GetUserType(dynamic userEntity)
    {
        if (userEntity.Driver != null) return "driver";
        if (userEntity.CargoOwner != null) return "cargoowner";
        if (userEntity.TruckOwner != null) return "truckowner";
        if (userEntity.Manager != null) return "manager";
        return "user";
    }

    private string GetUserTypeDisplay(dynamic userEntity)
    {
        if (userEntity.Driver != null) return "Driver";
        if (userEntity.CargoOwner != null) return "Cargo Owner";
        if (userEntity.TruckOwner != null) return "Truck Owner";
        if (userEntity.Manager != null) return "Manager";
        return "User";
    }

    private string? GetUserPhone(dynamic userEntity)
    {
        if (userEntity.Driver != null) return userEntity.Driver.Phone;
        if (userEntity.CargoOwner != null) return userEntity.CargoOwner.Phone;
        if (userEntity.TruckOwner != null) return userEntity.TruckOwner.Phone;
        if (userEntity.Manager != null) return userEntity.Manager.Phone;
        return null;
    }

    private string? GetCompanyName(dynamic userEntity)
    {
        if (userEntity.CargoOwner != null) return userEntity.CargoOwner.CompanyName;
        if (userEntity.TruckOwner != null) return userEntity.TruckOwner.Name; // TruckOwner uses Name field
        return null;
    }
}