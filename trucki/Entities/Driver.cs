using System.ComponentModel.DataAnnotations;

namespace trucki.Entities
{
    public class Driver : BaseClass
    {
        public string Name { get; set; }
        
        public string Phone { get; set; }

        [EmailAddress]
        public string EmailAddress { get; set; }

        public string? TruckId { get; set; }

        public string DriversLicence { get; set; }

        public string PassportFile { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
