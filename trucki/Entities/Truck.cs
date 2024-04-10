namespace trucki.Entities;

public class Truck : BaseClass
{
    public List<string> Documents { get; set; }
    //public string CertOfOwnerShip { get; set; }
    public string PlateNumber { get; set; }
    public string TruckCapacity { get; set; }
    public string? DriverId { get; set; }
    //public string Capacity { get; set; }    
    public string TruckOwnerId { get; set; }
    public TruckiType TruckType { get; set; }
    public string TruckLicenseExpiryDate { get; set; }
    public string RoadWorthinessExpiryDate { get; set; }
    public string InsuranceExpiryDate { get; set; }
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
