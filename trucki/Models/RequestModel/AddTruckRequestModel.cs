using trucki.Entities;

namespace trucki.Models.RequestModel
{
    public class AddTruckRequestModel
    {
        public List<IFormFile> Documents { get; set; }
        public string CertOfOwnerShip { get; set; }
        public string PlateNumber { get; set; }
        public string TruckCapacity { get; set; }
        public string? DriverId { get; set; }
        public string Capacity { get; set; }
        public string TruckOwnerId { get; set; }
        public TruckiType TruckType { get; set; }
        public DateOnly TruckLicenseExpiryDate { get; set; }
        public DateOnly RoadWorthinessExpiryDate { get; set; }
        public DateOnly InsuranceExpiryDate { get; set; }
    }

    public class EditTruckRequestModel
    {
        public string TruckId { get; set; }
        public List<IFormFile> Documents { get; set; }
        public string CertOfOwnerShip { get; set; }
        public string PlateNumber { get; set; }
        public string TruckCapacity { get; set; }
        public string? DriverId { get; set; }
        public string Capacity { get; set; }
        public string TruckOwnerId { get; set; }
        public TruckiType TruckType { get; set; }
        public DateOnly TruckLicenseExpiryDate { get; set; }
        public DateOnly RoadWorthinessExpiryDate { get; set; }
        public DateOnly InsuranceExpiryDate { get; set; }
    }
}
