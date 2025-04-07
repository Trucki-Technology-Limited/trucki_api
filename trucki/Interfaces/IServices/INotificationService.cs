// trucki/Interfaces/IServices/INotificationService.cs
namespace trucki.Interfaces.IServices;

public interface INotificationService
{
    Task SendNotificationAsync(string userId, string title, string body, Dictionary<string, string> data = null);
    Task SendNotificationToTopicAsync(string topic, string title, string body, Dictionary<string, string> data = null);
    Task SubscribeToTopicAsync(string token, string topic);
    Task UnsubscribeFromTopicAsync(string token, string topic);
    Task SaveDeviceTokenAsync(string userId, string token, string deviceType);
}