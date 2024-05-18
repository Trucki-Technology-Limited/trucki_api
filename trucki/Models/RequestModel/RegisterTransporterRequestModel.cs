namespace trucki.Models.RequestModel
{
    public class RegisterTransporterRequestModel
    {
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
        public string NumberOfDrivers { get; set; }
        public List<IFormFile> Documents { get; set; } 
        public string TruckType { get; set; }
        public string TruckCapacity { get; set; }
        public DateTime TruckLicenseExpiryDate { get; set; }
        public DateTime RoadWorthinessExpiryDate { get; set; }
        public DateTime InsruanceExpiryDate { get; set; }
        public string Password { get; set; }
    }

    public class RegisterFleetOwnerRequestModel
    {
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
        public string NumberOfDrivers { get; set; }
        public List<IFormFile> Documents { get; set; }
        public string TruckType { get; set; }
        public string TruckCapacity { get; set; }
        public DateTime TruckLicenseExpiryDate { get; set; }
        public DateTime RoadWorthinessExpiryDate { get; set; }
        public DateTime InsruanceExpiryDate { get; set; }
        public string Password { get; set; }
    }
}
