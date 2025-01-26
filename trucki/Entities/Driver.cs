using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace trucki.Entities
{
    public class Driver : BaseClass
    {
        public string Name { get; set; }
        
        public string Phone { get; set; }

        [EmailAddress]
        public string EmailAddress { get; set; }

        [ForeignKey("TruckId")]
        public string? TruckId { get; set; }
        public Truck? Truck { get; set; }
        public string? TruckOwnerId { get; set; }
        public TruckOwner? TruckOwner { get; set; }
        public string? DriversLicence { get; set; }

        public string? PassportFile { get; set; }
        public bool IsActive { get; set; } = true;
        [ForeignKey("User")] 
        public string? UserId { get; set; } 

        public User? User { get; set; }

           // New property to capture which country this driver belongs to (if relevant)
        public string Country { get; set; } = "NG";  // or "NG" or any default

        // (Optional) If you plan to use the same DocumentTypes approach:
        // you can also store the EntityType if each driver is "Driver" for sure:
        // public string EntityType { get; set; } = "Driver";

        // Navigation property for documents
        public ICollection<DriverDocument> DriverDocuments { get; set; } = new List<DriverDocument>();
    }
}
