using System.ComponentModel.DataAnnotations.Schema;

namespace trucki.Entities
{
    public class DriverWallet : BaseClass
    {
        public string DriverId { get; set; }
        
        [ForeignKey("DriverId")]
        public Driver Driver { get; set; }
        
        public decimal Balance { get; set; } = 0;
        public decimal PendingWithdrawal { get; set; } = 0; // Amount scheduled for this week's withdrawal
        public decimal NextWithdrawal { get; set; } = 0; // Amount scheduled for next week's withdrawal
        
        // Navigation property for transactions
        public ICollection<DriverWalletTransaction> Transactions { get; set; } = new List<DriverWalletTransaction>();
    }

    public class DriverWalletTransaction : BaseClass
    {
        public string WalletId { get; set; }
        
        [ForeignKey("WalletId")]
        public DriverWallet Wallet { get; set; }
        
        public decimal Amount { get; set; }  // Positive for credits, negative for debits
        
        public string Description { get; set; }
        
        public string? RelatedOrderId { get; set; }
        
        [ForeignKey("RelatedOrderId")]
        public CargoOrders? RelatedOrder { get; set; }
        
        public DriverTransactionType TransactionType { get; set; }

        // For withdrawals - track bank transfer reference
        public string? BankTransferReference { get; set; }
        
        // Flag for admin to mark transaction as processed (for withdrawals)
        public bool IsProcessed { get; set; } = false;
        
        // When a withdrawal was processed
        public DateTime? ProcessedAt { get; set; }
        
        // Who processed the withdrawal (admin user ID)
        public string? ProcessedBy { get; set; }
    }
    
    public enum DriverTransactionType
    {
        Delivery,      // Payment for completed delivery
        Withdrawal,    // Automatic withdrawal to bank account
        Refund,        // Refund (e.g., if order cancelled)
        Bonus,         // Any bonus payments
        Adjustment,    // Administrative adjustment
        Commission     // Any commission charged
    }
    
    public class DriverWithdrawalSchedule : BaseClass
    {
        public DateTime ScheduledDate { get; set; }
        public bool IsProcessed { get; set; } = false;
        public DateTime? ProcessedAt { get; set; }
        public string? ProcessedBy { get; set; }
        public decimal TotalAmount { get; set; }
        public int TransactionsCount { get; set; }
        public string Notes { get; set; }
        
        // Navigation property for all transactions in this withdrawal batch
        public ICollection<DriverWalletTransaction> Transactions { get; set; } = new List<DriverWalletTransaction>();
    }
}