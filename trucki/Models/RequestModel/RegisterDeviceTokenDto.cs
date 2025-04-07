// trucki/Models/RequestModel/RegisterDeviceTokenDto.cs
namespace trucki.Models.RequestModel;

public class RegisterDeviceTokenDto
{
    public string Token { get; set; }
    public string DeviceType { get; set; } // "android", "ios", "web"
}