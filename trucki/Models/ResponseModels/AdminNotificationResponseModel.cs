namespace trucki.Models.ResponseModels;

public class AdminNotificationResponseModel
{
    public int TotalTargetUsers { get; set; }
    public int SuccessfulPushNotifications { get; set; }
    public int FailedPushNotifications { get; set; }
    public int SuccessfulEmails { get; set; }
    public int FailedEmails { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}

public class UserTypeOptionResponseModel
{
    public string UserType { get; set; }
    public string DisplayName { get; set; }
    public int UserCount { get; set; }
}