using trucki.Entities;

namespace trucki.Models.RequestModel
{
    public class GetWalletTransactionsQueryDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public WalletTransactionType? TransactionType { get; set; }
        public bool SortDescending { get; set; } = true;
    }
}
