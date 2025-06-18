using trucki.Entities;

namespace trucki.Models.RequestModel
{
    public class CreateStripeConnectAccountRequestModel
    {
        public string DriverId { get; set; }
        public string? Email { get; set; } // Optional, will use driver's email if not provided
        public string? Country { get; set; } = "US"; // Default to US, can be overridden
        public string? BusinessType { get; set; } = "individual"; // individual or company
    }

    public class UpdateStripeAccountStatusRequestModel
    {
        public string DriverId { get; set; }
        public string StripeAccountId { get; set; }
        public string Status { get; set; } // From Stripe webhook
    }

    public class FlagOrderRequestModel
    {
        public string OrderId { get; set; }
        public string Reason { get; set; }
        public OrderFlagType FlagType { get; set; }
        public string AdminId { get; set; }
    }

    public class ResolveFlagRequestModel
    {
        public string OrderId { get; set; }
        public string ResolutionNotes { get; set; }
        public string AdminId { get; set; }
    }

    public class ProcessPayoutRequestModel
    {
        public string? DriverId { get; set; } // If null, process for all eligible drivers
        public bool ForceProcess { get; set; } = false; // Override normal schedule
        public string AdminId { get; set; }
    }
}
