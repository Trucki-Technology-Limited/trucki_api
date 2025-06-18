using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace trucki.Entities
{
    public class DriverPayout : BaseClass
    {
        [ForeignKey("Driver")]
        public string DriverId { get; set; }
        public Driver Driver { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
        
        // Payout period information
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public DateTime ProcessedDate { get; set; }
        
        // Stripe information
        public string? StripeTransferId { get; set; }
        public string? StripePayoutId { get; set; }
        
        public PayoutStatus Status { get; set; } = PayoutStatus.Pending;
        public string? FailureReason { get; set; }
        
        // Metadata
        public int OrdersCount { get; set; } // Number of orders included in this payout
        public string? ProcessedBy { get; set; } // Admin ID or "system" for automated
        public string? Notes { get; set; }
        
        // Related orders for this payout
        public ICollection<DriverPayoutOrder> PayoutOrders { get; set; } = new List<DriverPayoutOrder>();
    }

    public class DriverPayoutOrder : BaseClass
    {
        [ForeignKey("DriverPayout")]
        public string DriverPayoutId { get; set; }
        public DriverPayout DriverPayout { get; set; }

        [ForeignKey("CargoOrder")]
        public string OrderId { get; set; }
        public CargoOrders Order { get; set; }

        public decimal OrderEarnings { get; set; } // Driver's earnings from this specific order
        public DateTime OrderCompletedDate { get; set; }
    }

    public enum PayoutStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Cancelled
    }
}