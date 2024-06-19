namespace trucki.Models.ResponseModels;

public class TransactionSummaryResponseModel
{
        public int TotalOrderCount { get; set; }
        public decimal TotalPayout { get; set; }
        public decimal TotalGtv { get; set; }
}