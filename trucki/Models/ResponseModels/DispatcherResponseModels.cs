using trucki.Entities;

namespace trucki.Models.ResponseModels;

// Base dispatcher response model
public class DispatcherResponseModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string EmailAddress { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public string Country { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalDrivers { get; set; }
    public int ActiveDrivers { get; set; }
    public decimal TotalEarnings { get; set; }
}

// For CargoOwner - they see driver information with dispatcher indicator
public class CargoOwnerBidResponseModel
{
    public string Id { get; set; }
    public decimal Amount { get; set; }
    public BidStatus Status { get; set; }
    public string DriverId { get; set; }
    public string DriverName { get; set; }
    public string TruckId { get; set; }
    public CargoTruckResponseModel Truck { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
    public bool IsFromDispatcher { get; set; } // Simple indicator for cargo owner
}

// For Admin - they see both driver and dispatcher information
public class AdminBidResponseModel
{
    public string Id { get; set; }
    public decimal Amount { get; set; }
    public BidStatus Status { get; set; }
    public string DriverId { get; set; }
    public string DriverName { get; set; }
    public string TruckId { get; set; }
    public CargoTruckResponseModel Truck { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }

    // Additional dispatcher information for admin
    public BidSubmitterType SubmitterType { get; set; }
    public string? SubmittedByDispatcherId { get; set; }
    public string? DispatcherName { get; set; }
    public decimal? DispatcherCommissionAmount { get; set; }
}

// Driver response for dispatcher dashboard
public class DispatcherDriverResponseModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string EmailAddress { get; set; }
    public string Country { get; set; }
    public DriverOnboardingStatus OnboardingStatus { get; set; }
    public DriverOwnershipType OwnershipType { get; set; }
    public decimal CommissionPercentage { get; set; }
    public DateTime CommissionEffectiveFrom { get; set; }
    public bool HasTruck { get; set; }
    public string? TruckId { get; set; }
    public string? TruckName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Commission calculation result
public class CommissionCalculationResult
{
    public decimal TotalBidAmount { get; set; }
    public decimal SystemFee { get; set; }
    public decimal NetAmount { get; set; }
    public decimal DispatcherCommission { get; set; }
    public decimal DriverEarnings { get; set; }
    public decimal CommissionPercentage { get; set; }
}

// Dispatcher dashboard summary
public class DispatcherDashboardResponseModel
{
    public DispatcherResponseModel DispatcherInfo { get; set; }
    public int TotalDrivers { get; set; }
    public int ActiveOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal MonthlyEarnings { get; set; }
    public List<DispatcherDriverResponseModel> RecentDrivers { get; set; }
    public List<CargoOrderSummaryResponseModel> RecentOrders { get; set; }
}

// Simple order summary for dashboard
public class CargoOrderSummaryResponseModel
{
    public string Id { get; set; }
    public string PickupLocation { get; set; }
    public string DeliveryLocation { get; set; }
    public CargoOrderStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string DriverName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PickupDateTime { get; set; }
}

// Driver commission response models
public class DriverCommissionResponseModel
{
    public string Id { get; set; }
    public string DriverId { get; set; }
    public string DriverName { get; set; }
    public string DispatcherId { get; set; }
    public string DispatcherName { get; set; }
    public decimal CommissionPercentage { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}

public class DriverCommissionHistoryResponseModel
{
    public string DriverId { get; set; }
    public string DriverName { get; set; }
    public string DispatcherId { get; set; }
    public DriverCommissionResponseModel CurrentCommission { get; set; }
    public List<DriverCommissionResponseModel> CommissionHistory { get; set; }
}

// All enums are imported from trucki.Entities
// TruckResponseModel should be imported from existing response models