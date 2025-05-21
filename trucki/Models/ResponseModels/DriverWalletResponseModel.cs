using trucki.Entities;

namespace trucki.Models.ResponseModels
{
    public class DriverWalletBalanceResponseModel
    {
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal PendingWithdrawal { get; set; } // Amount for upcoming Friday
        public decimal NextWithdrawal { get; set; } // Amount for the following Friday
        public DateTime NextWithdrawalDate { get; set; } // Date of upcoming withdrawal
        public int CompletedOrders { get; set; }
        public decimal TotalEarnings { get; set; }
        
        // Last few transactions
        public List<DriverWalletTransactionResponseModel> RecentTransactions { get; set; } = new List<DriverWalletTransactionResponseModel>();
    }
    
    public class DriverWalletTransactionResponseModel
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string RelatedOrderId { get; set; }
        public DriverTransactionType TransactionType { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsProcessed { get; set; } // For withdrawals
        public DateTime? ProcessedAt { get; set; }
        public string BankTransferReference { get; set; }
        
        // Extra order detail for UI display
        public string OrderPickupLocation { get; set; }
        public string OrderDeliveryLocation { get; set; }
    }
    
    public class DriverWithdrawalScheduleResponseModel
    {
        public DateTime CurrentWeekCutoffDate { get; set; } // Thursday of current week
        public DateTime NextWithdrawalDate { get; set; } // Friday of current week
        public decimal AmountDueThisWeek { get; set; }
        public decimal AmountDueNextWeek { get; set; }
        public List<ScheduledWithdrawalItemResponseModel> UpcomingWithdrawals { get; set; } = new List<ScheduledWithdrawalItemResponseModel>();
        public List<DriverWalletTransactionResponseModel> PastWithdrawals { get; set; } = new List<DriverWalletTransactionResponseModel>();
    }
    
    public class ScheduledWithdrawalItemResponseModel
    {
        public DateTime ScheduledDate { get; set; }
        public decimal Amount { get; set; }
        public int DeliveryCount { get; set; }
        public List<string> OrderIds { get; set; } = new List<string>();
    }
    
    public class PendingDriverWithdrawalResponseModel
    {
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal Amount { get; set; }
        public int OrderCount { get; set; }
        public DateTime ScheduledDate { get; set; }
    }
    
    public class DriverWithdrawalResultModel
    {
        public int ProcessedCount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<DriverWithdrawalError> Errors { get; set; } = new List<DriverWithdrawalError>();
        public string BatchReference { get; set; }
        public DateTime ProcessedDate { get; set; }
    }
    
    public class DriverWithdrawalError
    {
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public string ErrorMessage { get; set; }
    }
    
    public class DriverEarningsSummaryResponseModel
    {
        public decimal TotalEarnings { get; set; }
        public int CompletedDeliveries { get; set; }
        public decimal AveragePerDelivery { get; set; }
        public decimal HighestPayout { get; set; }
        public decimal WithdrawnAmount { get; set; }
        public decimal AvailableBalance { get; set; }
        public List<EarningsByWeekResponseModel> WeeklyBreakdown { get; set; } = new List<EarningsByWeekResponseModel>();
    }
    
    public class EarningsByWeekResponseModel
    {
        public int WeekNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal EarningsAmount { get; set; }
        public int DeliveryCount { get; set; }
    }
}