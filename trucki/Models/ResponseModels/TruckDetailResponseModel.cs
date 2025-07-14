using trucki.Entities;

namespace trucki.Models.ResponseModels
{
    /// <summary>
    /// Detailed truck response model for GetTruckById endpoint with complete driver and order information
    /// </summary>
    public class TruckDetailResponseModel
    {
        // Basic Truck Information
        public string Id { get; set; }
        public string PlateNumber { get; set; }
        public string? TruckName { get; set; }
        public string TruckType { get; set; }
        public string TruckCapacity { get; set; }
        public string TruckLicenseExpiryDate { get; set; }
        public string RoadWorthinessExpiryDate { get; set; }
        public string InsuranceExpiryDate { get; set; }
        public TruckStatus TruckStatus { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
        public bool IsDriverOwnedTruck { get; set; }
        public List<string>? Documents { get; set; }
        public string? ExternalTruckPictureUrl { get; set; }
        public string? CargoSpacePictureUrl { get; set; }
        public string? TruckiNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Driver Information (if driver-owned or assigned)
        public DriverDetailInfo? DriverDetails { get; set; }
        
        // Truck Owner Information (if truck owner-owned)
        public TruckOwnerDetailInfo? TruckOwnerDetails { get; set; }
        
        // Trip and Order Statistics
        public TruckOrderStatistics OrderStatistics { get; set; }
        
        // Current Trip Information
        public CurrentTripInfo? CurrentTrip { get; set; }
    }
    
    /// <summary>
    /// Detailed driver information for truck details
    /// </summary>
    public class DriverDetailInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string EmailAddress { get; set; }
        public string Country { get; set; }
        public bool IsActive { get; set; }
        public string? DriversLicence { get; set; }
        public string? PassportFile { get; set; }
        public DriverOnboardingStatus OnboardingStatus { get; set; }
    }
    
    /// <summary>
    /// Truck owner information for truck details
    /// </summary>
    public class TruckOwnerDetailInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string? EmailAddress { get; set; }
        public string Address { get; set; }
    }
    
    /// <summary>
    /// Truck order and trip statistics
    /// </summary>
    public class TruckOrderStatistics
    {
        public int TotalCargoOrders { get; set; }
        public int CompletedCargoOrders { get; set; }
        public int TotalNormalOrders { get; set; }
        public int CompletedNormalOrders { get; set; }
        public int TotalTrips => TotalCargoOrders + TotalNormalOrders;
        public int CompletedTrips => CompletedCargoOrders + CompletedNormalOrders;
        public bool IsCurrentlyOnTrip { get; set; }
        public decimal TotalEarnings { get; set; }
        public DateTime? LastTripDate { get; set; }
    }
    
    /// <summary>
    /// Current trip information if truck is on a trip
    /// </summary>
    public class CurrentTripInfo
    {
        public string OrderId { get; set; }
        public string OrderType { get; set; } // "Cargo" or "Normal"
        public string Status { get; set; }
        public string? PickupLocation { get; set; }
        public string? DeliveryLocation { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? CargoType { get; set; }
        public string? CustomerName { get; set; }
    }
}