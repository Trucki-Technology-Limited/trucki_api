using System.ComponentModel.DataAnnotations.Schema;

namespace trucki.Entities
{
    public class Officer : BaseClass
    {
        public string OfficerName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public OfficerType OfficerType { get; set; }
        public string? CompanyId { get; set; }
        public bool IsActive { get; set; } = true;
        [ForeignKey("User")]
        public string? UserId { get; set; }

        public User? User { get; set; }

    }

    public enum OfficerType
    {
        FieldOfficer,
        SafetyOfficer
    }
}
