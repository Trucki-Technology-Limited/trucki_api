// trucki/Entities/DeviceToken.cs
namespace trucki.Entities;

public class DeviceToken : BaseClass
{
    public string UserId { get; set; }
    public string Token { get; set; }
    public string DeviceType { get; set; } // "android", "ios", "web"
}