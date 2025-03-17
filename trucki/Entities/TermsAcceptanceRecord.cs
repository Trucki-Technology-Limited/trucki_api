using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace trucki.Entities
{
    public class TermsAcceptanceRecord : BaseClass
    {
        [ForeignKey("Driver")]
        public string DriverId { get; set; }
        public Driver Driver { get; set; }
        
        public string TermsVersion { get; set; }
        public DateTime AcceptedAt { get; set; }
        
        // Optional: IP address or device info for audit purposes
        public string? AcceptedFromIp { get; set; }
        public string? DeviceInfo { get; set; }
        
        // You could also store the full text of the terms that were accepted
        // public string TermsText { get; set; }
    }
}