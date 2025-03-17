namespace trucki.Models.ResponseModels;
public class TermsAcceptanceRecordDto
{
    public string Id { get; set; }
    public string DriverId { get; set; }
    public string DriverName { get; set; } // Optional, for admin display
    public string TermsVersion { get; set; }
    public DateTime AcceptedAt { get; set; }
    public string AcceptedFromIp { get; set; }
    public string DeviceInfo { get; set; }
}