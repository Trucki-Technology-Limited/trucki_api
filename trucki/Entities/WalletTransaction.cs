using System.ComponentModel.DataAnnotations.Schema;

namespace trucki.Entities
{
    public class WalletTransaction : BaseClass
    {
        public string WalletId { get; set; }

        [ForeignKey("WalletId")]
        public CargoOwnerWallet Wallet { get; set; }

        public decimal Amount { get; set; }  // Positive for credits, negative for debits

        public string Description { get; set; }

        public string? RelatedOrderId { get; set; }

        [ForeignKey("RelatedOrderId")]
        public CargoOrders? RelatedOrder { get; set; }

        public WalletTransactionType TransactionType { get; set; }
    }

    public enum WalletTransactionType
    {
        TopUp = 1,
        Payment = 2,
        Refund = 3,
        Transfer = 4,
        Withdrawal = 5,
        CancellationFee = 6
    }
}