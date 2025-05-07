using trucki.Entities;

namespace trucki.Models.ResponseModels
{
    public class WalletTransactionResponseModel
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string RelatedOrderId { get; set; }
        public WalletTransactionType TransactionType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    public class WalletBalanceResponseModel
    {
        public decimal Balance { get; set; }
        public decimal PendingBalance { get; set; }
        public decimal AvailableBalance { get; set; }
    }
}