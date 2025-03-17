public class AcceptTermsRequestModel
{
    public string DriverId { get; set; }
    public string TermsVersion { get; set; } = "2025"; // Default to current version
    public DateTime AcceptedAt { get; set; } = DateTime.UtcNow;
    
    // Optional fields for audit
    public string? IpAddress { get; set; }
    public string? DeviceInfo { get; set; }
}