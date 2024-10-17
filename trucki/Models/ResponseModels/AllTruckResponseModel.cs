using trucki.Entities;

namespace trucki.Models.ResponseModels
{
    public class AllTruckResponseModel
    {
        public string Id { get; set; }
        public List<string> Documents { get; set; }
        public string CertOfOwnerShip { get; set; }
        public string PlateNumber { get; set; }
        public string TruckCapacity { get; set; }
        public string? DriverId { get; set; }
        public string? DriverName { get; set; }
        public string Capacity { get; set; }
        public string TruckOwnerId { get; set; }
        public string TruckOwnerName { get; set; }
        public TruckiType TruckType { get; set; }
        public string TruckLicenseExpiryDate { get; set; }
        public string RoadWorthinessExpiryDate { get; set; }
        public string InsuranceExpiryDate { get; set; }
        public string TruckNumber { get; set; }
    }
}
