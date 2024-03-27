using System.ComponentModel.DataAnnotations;

namespace trucki.Entities
{
    public class Manager : BaseClass

    {
        public string Name { get; set; }
        public string Phone { get; set; }

        [EmailAddress]
        public string EmailAddress { get; set; }

        public List<Business> Company { get; set; }

        public ManagerType ManagerType { get; set; }
        public bool IsActive { get; set; } = true; 
    }

    public enum ManagerType
    {
        OperationalManager,
        FinancialManager
    }
}
