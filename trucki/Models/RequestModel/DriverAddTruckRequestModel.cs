using trucki.Entities;

namespace trucki.Models.RequestModel;

public class DriverAddTruckRequestModel
{
    public string DriverId { get; set; }
    public string PlateNumber { get; set; }
    public string? TruckName { get; set; }
    public string TruckCapacity { get; set; }
    public TruckiType TruckType { get; set; }
    public string TruckLicenseExpiryDate { get; set; }
    public string RoadWorthinessExpiryDate { get; set; }
    public string InsuranceExpiryDate { get; set; }
    public List<string> Documents { get; set; }
    
    // New fields for pictures
    public string ExternalTruckPictureUrl { get; set; }
    public string CargoSpacePictureUrl { get; set; }
}