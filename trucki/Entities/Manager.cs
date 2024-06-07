using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [ForeignKey("User")] 
        public string? UserId { get; set; } 

        public User? User { get; set; }
    }

    public enum ManagerType
    {
        OperationalManager,
        FinancialManager
    }
}
