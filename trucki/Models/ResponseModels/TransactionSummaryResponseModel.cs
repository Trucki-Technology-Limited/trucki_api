namespace trucki.Models.ResponseModels;

public class TransactionSummaryResponseModel
{
        public int TotalOrderCount { get; set; }
        public decimal TotalPayout { get; set; }
        public decimal TotalGtv { get; set; }
        public decimal TotalPrice { get; set; } // New field for sum of route prices
        public decimal TotalRevenue { get; set; } // Calculated as TotalGtv - TotalPrice
}