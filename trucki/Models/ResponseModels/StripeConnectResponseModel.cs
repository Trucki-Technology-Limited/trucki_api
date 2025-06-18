using trucki.Entities;

namespace trucki.Models.ResponseModels
{
    public class StripeConnectAccountResponseModel
    {
        public string DriverId { get; set; }
        public string StripeAccountId { get; set; }
        public string OnboardingUrl { get; set; }
        public string ReturnUrl { get; set; }
        public string RefreshUrl { get; set; }
        public StripeAccountStatus Status { get; set; }
        public bool CanReceivePayouts { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DriverPayoutResponseModel
    {
        public string Id { get; set; }
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public DateTime ProcessedDate { get; set; }
        public PayoutStatus Status { get; set; }
        public int OrdersCount { get; set; }
        public string? StripeTransferId { get; set; }
        public string? FailureReason { get; set; }
        public List<PayoutOrderDetailModel> Orders { get; set; } = new List<PayoutOrderDetailModel>();
    }

    public class PayoutOrderDetailModel
    {
        public string OrderId { get; set; }
        public decimal OrderEarnings { get; set; }
        public DateTime CompletedDate { get; set; }
        public string PickupLocation { get; set; }
        public string DeliveryLocation { get; set; }
    }

    public class PayoutSummaryResponseModel
    {
        public int TotalDrivers { get; set; }
        public int EligibleDrivers { get; set; }
        public int ProcessedPayouts { get; set; }
        public int FailedPayouts { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime ProcessedAt { get; set; }
        public List<PayoutErrorModel> Errors { get; set; } = new List<PayoutErrorModel>();
    }

    public class PayoutErrorModel
    {
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public string ErrorMessage { get; set; }
        public decimal AttemptedAmount { get; set; }
    }

    public class DriverEarningsProjectionModel
    {
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public decimal CurrentWeekEarnings { get; set; }
        public decimal NextPayoutAmount { get; set; }
        public DateTime NextPayoutDate { get; set; }
        public int PendingOrdersCount { get; set; }
        public List<PendingOrderEarningsModel> PendingOrders { get; set; } = new List<PendingOrderEarningsModel>();
    }

    public class PendingOrderEarningsModel
    {
        public string OrderId { get; set; }
        public decimal Earnings { get; set; }
        public DateTime CompletedDate { get; set; }
        public bool IsFlagged { get; set; }
        public string? FlagReason { get; set; }
    }
}