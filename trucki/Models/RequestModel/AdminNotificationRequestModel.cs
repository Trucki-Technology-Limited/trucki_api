using System.ComponentModel.DataAnnotations;
using trucki.Entities;

namespace trucki.Models.RequestModel;

public class AdminNotificationRequestModel
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; }

    [Required]
    public NotificationChannelType Channel { get; set; }

    [Required]
    public NotificationTargetType TargetType { get; set; }

    // Only used when TargetType is SpecificUsers
    public List<string>? UserIds { get; set; }

    // Only used when TargetType is UserType
    public string? UserType { get; set; }

    // Optional data for push notifications
    public Dictionary<string, string>? Data { get; set; }
}

public class AdminEmailNotificationRequestModel
{
    [Required]
    [MaxLength(200)]
    public string Subject { get; set; }

    [Required]
    public string HtmlBody { get; set; }

    [Required]
    public NotificationTargetType TargetType { get; set; }

    // Only used when TargetType is SpecificUsers
    public List<string>? UserIds { get; set; }

    // Only used when TargetType is UserType
    public string? UserType { get; set; }
}

public enum NotificationChannelType
{
    PushOnly,
    EmailOnly,
    Both
}

public enum NotificationTargetType
{
    AllUsers,
    SpecificUsers,
    UserType
}