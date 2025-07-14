using trucki.Entities;

namespace trucki.Models.ResponseModels
{
    /// <summary>
    /// Enhanced truck response model with additional details including driver info and trip statistics
    /// </summary>
    public class EnhancedTruckResponseModel
    {
        public string Id { get; set; }
        public string PlateNumber { get; set; }
        public string? TruckName { get; set; }
        public string TruckType { get; set; }
        public string TruckLicenseExpiryDate { get; set; }
        public string InsuranceExpiryDate { get; set; }
        public TruckStatus TruckStatus { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
        
        // Driver details (populated when IsDriverOwnedTruck = true)
        public string? DriverId { get; set; }
        public string? DriverName { get; set; }
        public string? DriverPhone { get; set; }
        public string? DriverEmail { get; set; }
        
        // Truck Owner details (populated when truck is owned by truck owner)
        public string? TruckOwnerId { get; set; }
        public string? TruckOwnerName { get; set; }
        
        // Trip/Order statistics
        public int TotalCargoOrders { get; set; }
        public int TotalNormalOrders { get; set; }
        public bool IsCurrentlyOnTrip { get; set; }
        public string? CurrentTripOrderId { get; set; }
        public string? CurrentTripType { get; set; } // "Cargo" or "Normal"
        
        // Additional truck info
        public string TruckCapacity { get; set; }
        public bool IsDriverOwnedTruck { get; set; }
        public List<string>? Documents { get; set; }
        public string? ExternalTruckPictureUrl { get; set; }
        public string? CargoSpacePictureUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}