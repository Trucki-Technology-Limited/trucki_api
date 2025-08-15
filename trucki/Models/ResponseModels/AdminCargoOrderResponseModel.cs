using trucki.Entities;

namespace trucki.Models.ResponseModels
{
    /// <summary>
    /// Response model for admin cargo orders list view
    /// </summary>
    public class AdminCargoOrderResponseModel
    {
        public string Id { get; set; }
        public string CargoOwnerId { get; set; }
        public string CargoOwnerName { get; set; }
        public string CargoOwnerEmail { get; set; }
        public string CargoOwnerCompany { get; set; }
        public string PickupLocation { get; set; }
        public string DeliveryLocation { get; set; }
        public CargoOrderStatus Status { get; set; }
        public string StatusDisplay { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string PaymentStatusDisplay { get; set; }
        public int ItemCount { get; set; }
        public int BidCount { get; set; }
        public bool HasAcceptedBid { get; set; }
        public string? AcceptedBidId { get; set; }
        public decimal? AcceptedBidAmount { get; set; }
        public string? AcceptedDriverName { get; set; }
        public string? AcceptedTruckPlateNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SystemFee { get; set; }
        public decimal Tax { get; set; }
        public string? Consignment { get; set; }
        public DateTime? PickupDateTime { get; set; }
        public DateTime? DeliveryDateTime { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime? PaymentDueDate { get; set; }
        public bool IsPaid { get; set; }
        public PaymentMethodType PaymentMethod { get; set; }
        public decimal? WalletPaymentAmount { get; set; }
        public decimal? StripePaymentAmount { get; set; }
        public bool IsFlagged { get; set; }
        public string? FlagReason { get; set; }
        public DateTime? FlaggedAt { get; set; }
        public string? FlaggedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Calculate derived properties
        public string AgeDisplay => GetAgeDisplay();
        public string PaymentOverdueDisplay => GetPaymentOverdueDisplay();
        public bool IsOverdue => PaymentDueDate.HasValue && PaymentDueDate < DateTime.UtcNow && !IsPaid;
        
        private string GetAgeDisplay()
        {
            var age = DateTime.UtcNow - CreatedAt;
            if (age.TotalDays >= 1)
                return $"{(int)age.TotalDays} day{((int)age.TotalDays != 1 ? "s" : "")} ago";
            if (age.TotalHours >= 1)
                return $"{(int)age.TotalHours} hour{((int)age.TotalHours != 1 ? "s" : "")} ago";
            return $"{(int)age.TotalMinutes} minute{((int)age.TotalMinutes != 1 ? "s" : "")} ago";
        }
        
        private string GetPaymentOverdueDisplay()
        {
            if (!PaymentDueDate.HasValue || IsPaid)
                return string.Empty;
                
            var overdue = DateTime.UtcNow - PaymentDueDate.Value;
            if (overdue.TotalDays > 0)
                return $"Overdue by {(int)overdue.TotalDays} day{((int)overdue.TotalDays != 1 ? "s" : "")}";
                
            var dueIn = PaymentDueDate.Value - DateTime.UtcNow;
            if (dueIn.TotalDays > 0)
                return $"Due in {(int)dueIn.TotalDays} day{((int)dueIn.TotalDays != 1 ? "s" : "")}";
                
            return "Due today";
        }
    }

    /// <summary>
    /// Detailed response model for admin cargo order details view
    /// </summary>
    public class AdminCargoOrderDetailsResponseModel : AdminCargoOrderResponseModel
    {
        public AdminCargoOwnerSummaryModel CargoOwner { get; set; }
        public List<CargoOrderItemDetailsModel> Items { get; set; } = new();
        public List<AdminBidDetailsModel> Bids { get; set; } = new();
        public AdminBidDetailsModel? AcceptedBid { get; set; }
        public List<string> Documents { get; set; } = new();
        public List<string> DeliveryDocuments { get; set; } = new();
        public string PickupLocationLat { get; set; }
        public string PickupLocationLong { get; set; }
        public string DeliveryLocationLat { get; set; }
        public string DeliveryLocationLong { get; set; }
        public DateTime? ActualPickupDateTime { get; set; }
        public string? PaymentIntentId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal? DriverEarnings { get; set; }
        public AdminCargoOrderAuditTrail AuditTrail { get; set; }
    }

    public class AdminCargoOwnerSummaryModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public CargoOwnerType OwnerType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class CargoOrderItemDetailsModel
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public decimal Weight { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal Volume => Length * Width * Height;
        public bool IsFragile { get; set; }
        public string? SpecialHandlingInstructions { get; set; }
        public CargoType Type { get; set; }
        public int Quantity { get; set; }
        public List<string> ItemImages { get; set; } = new();
    }

    public class AdminBidDetailsModel
    {
        public string Id { get; set; }
        public string TruckId { get; set; }
        public string TruckPlateNumber { get; set; }
        public string TruckType { get; set; }
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public string DriverPhone { get; set; }
        public string? TruckOwnerId { get; set; }
        public string? TruckOwnerName { get; set; }
        public decimal Amount { get; set; }
        public BidStatus Status { get; set; }
        public string? BidNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsAccepted { get; set; }
    }

    /// <summary>
    /// Cargo order statistics for admin dashboard
    /// </summary>
    public class AdminCargoOrderStatisticsResponseModel
    {
        public int TotalOrders { get; set; }
        public int TotalOrdersThisMonth { get; set; }
        public int TotalOrdersLastMonth { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalRevenueThisMonth { get; set; }
        public decimal TotalRevenueLastMonth { get; set; }
        public decimal TotalSystemFees { get; set; }
        public decimal TotalTaxes { get; set; }
        public int ActiveOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int FlaggedOrders { get; set; }
        public int OverduePayments { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<CargoOrderStatusCount> StatusBreakdown { get; set; } = new();
        public List<PaymentStatusCount> PaymentStatusBreakdown { get; set; } = new();
        public List<MonthlyOrderTrend> MonthlyTrends { get; set; } = new();
        public DateTime? LastUpdated { get; set; }
        
        // Growth calculations
        public decimal OrderGrowthPercentage => CalculateGrowthPercentage(TotalOrdersThisMonth, TotalOrdersLastMonth);
        public decimal RevenueGrowthPercentage => CalculateGrowthPercentage(TotalRevenueThisMonth, TotalRevenueLastMonth);
        
        private decimal CalculateGrowthPercentage(decimal current, decimal previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return Math.Round(((current - previous) / previous) * 100, 2);
        }
    }

    public class CargoOrderStatusCount
    {
        public CargoOrderStatus Status { get; set; }
        public string StatusDisplay { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class PaymentStatusCount
    {
        public PaymentStatus Status { get; set; }
        public string StatusDisplay { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class MonthlyOrderTrend
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class CargoOrderStatusSummaryModel
    {
        public CargoOrderStatus Status { get; set; }
        public string StatusDisplay { get; set; }
        public int Count { get; set; }
        public decimal TotalValue { get; set; }
    }

    /// <summary>
    /// Audit trail for cargo order changes
    /// </summary>
    public class AdminCargoOrderAuditTrail
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? LastStatusChange { get; set; }
        public DateTime? LastPaymentUpdate { get; set; }
        public DateTime? FlaggedAt { get; set; }
        public string? FlaggedBy { get; set; }
        public List<CargoOrderAuditEntry> StatusHistory { get; set; } = new();
    }

    public class CargoOrderAuditEntry
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Reason { get; set; }
    }
}

// Request models for admin actions
namespace trucki.Models.RequestModel
{
    public class FlagCargoOrderRequestModel
    {
        public bool IsFlagged { get; set; }
        public string? FlagReason { get; set; }
    }

    public class UpdateCargoOrderStatusRequestModel
    {
        public CargoOrderStatus Status { get; set; }
        public string? Reason { get; set; }
    }
}