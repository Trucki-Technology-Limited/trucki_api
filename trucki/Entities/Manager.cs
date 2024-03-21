using System.ComponentModel.DataAnnotations;

namespace trucki.Entities
{
    public class Manager : BaseClass

    {
        public string? ManagerName { get; set; }

        public string PhoneNumber { get; set; }

        [EmailAddress]
        public string EmailAddress { get; set; }

        public ManagerType ManagerType { get; set; }
    }

    public enum ManagerType
    {
        OperationalManager,
        FinancialManager
    }
}
