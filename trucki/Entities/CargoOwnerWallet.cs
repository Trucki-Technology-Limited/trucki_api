using System.ComponentModel.DataAnnotations.Schema;

namespace trucki.Entities
{
    public class CargoOwnerWallet : BaseClass
    {
        public string CargoOwnerId { get; set; }
        
        [ForeignKey("CargoOwnerId")]
        public CargoOwner CargoOwner { get; set; }
        
        public decimal Balance { get; set; } = 0;
        
        // Navigation property for transactions
        public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
    }
}