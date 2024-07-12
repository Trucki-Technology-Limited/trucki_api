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

        public string DriversLicence { get; set; }

        public string PassportFile { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
