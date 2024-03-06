using System.ComponentModel.DataAnnotations;

namespace trucki.Entities
{
    public class Manager : BaseClass

    {
        public string? ManagerName { get; set; }

       // [MaxLength(11)]
        public string PhoneNumber { get; set; }

        [EmailAddress]
        public string EmailAddress { get; set; }

        //public string Company { get; set; }

        // Foreign key
        public string CompanyId { get; set; }
        // Navigation property
        public Company Company { get; set; }

        public ManagerType ManagerType { get; set; }
    }

    public enum ManagerType
    {
        OperationalManager,
        FinancialManager
    }
}
