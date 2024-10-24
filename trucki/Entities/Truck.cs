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
}

public enum TruckiType
{
    Flatbed,
    BoxBody,
    BucketBody,
    Lowbed,
    ContainerizedBody,
    Refrigerator
}

public enum TruckStatus
{
    EnRoute,
    Available, 
    OutOfService,
}

public enum ApprovalStatus
{
    Approved,
    NotApproved, 
    Blocked,
}