using System.ComponentModel.DataAnnotations.Schema;

namespace trucki.Entities;

public class Truck : BaseClass
{
    public List<string> Documents { get; set; }
    //public string CertOfOwnerShip { get; set; }
    public string PlateNumber { get; set; }
    public string? TruckName { get; set; }
    public string TruckCapacity { get; set; }
    [ForeignKey("DriverId")]
    public string? DriverId { get; set; }
    public Driver? Driver { get; set; }
    public string? TruckOwnerId { get; set; }
    public string? TruckiNumber { get; set; }
    public TruckOwner? TruckOwner { get; set; }
    public TruckiType TruckType { get; set; }
    public string TruckLicenseExpiryDate { get; set; }
    public string RoadWorthinessExpiryDate { get; set; }
    public string InsuranceExpiryDate { get; set; }
    public TruckStatus TruckStatus { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; }
     // New fields for truck pictures
    public string? ExternalTruckPictureUrl { get; set; }
    public string? CargoSpacePictureUrl { get; set; }
    
    // Flag to identify if the truck was added by a driver (vs a truck owner)
    public bool IsDriverOwnedTruck { get; set; } = false;
}

public enum TruckiType
{
    Flatbed,
    BoxBody,
    BucketBody,
    Lowbed,
    ContainerizedBody,
    Refrigerator,
    CargoVan
}

public enum TruckStatus
{
    EnRoute,
    Available,
    Busy,
    OutOfService,
}

public enum ApprovalStatus
{
    Pending,
    Approved,
    NotApproved,
    Blocked,
}