using trucki.Entities;

namespace trucki.Models.RequestModel
{
    public class GetDriverWalletTransactionsQueryDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DriverTransactionType? TransactionType { get; set; }
        public bool SortDescending { get; set; } = true;
        public bool OnlyWithdrawals { get; set; } = false;
        public bool OnlyEarnings { get; set; } = false;
    }
    
    public class CreditDriverWalletRequestDto
    {
        public string DriverId { get; set; }
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
    
    public class ProcessDriverWithdrawalRequestDto
    {
        public string AdminId { get; set; }
        public DateTime? OverrideDate { get; set; } // For testing or manual processing
        public bool DryRun { get; set; } = false; // To preview what would be processed without actual changes
        public string Notes { get; set; }
    }
    
    public class DriverEarningsSummaryRequestDto
    {
        public string DriverId { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow.AddMonths(-1);
        public DateTime EndDate { get; set; } = DateTime.UtcNow;
        public bool IncludeWeeklyBreakdown { get; set; } = true;
    }
}