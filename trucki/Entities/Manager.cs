using System.ComponentModel.DataAnnotations;

namespace trucki.Models
{
    public class Manager : BaseClass

    {
        public string Name { get; set; }

        [MaxLength(11)]
        public int Phone { get; set; }

        [EmailAddress]
        public string EmailAddress { get; set; }

        public string Company { get; set; }

        public ManagerType ManagerType { get; set; }
    }

    public enum ManagerType
    {
        OperationalManager,
        FinancialManager
    }
}
