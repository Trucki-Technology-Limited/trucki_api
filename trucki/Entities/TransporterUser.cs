namespace trucki.Entities
{
    public class TransporterUser
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); 
        public string UserId { get; set; }
        public User User { get; set; }
        public string Location { get; set; }
        public string NumberOfDrivers { get; set; }
        public string TruckType { get; set; }
        public string TruckCapacity { get; set; }
        public DateTime TruckLicenseExpiryDate { get; set; }
        public DateTime RoadWorthinessExpiryDate { get; set; }
        public DateTime InsruanceExpiryDate { get; set; }
        public List<string> Documents { get; set; }

    }

    public class FleetOwnerUser
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public User User { get; set; }
        public string Location { get; set; }
        public string NumberOfDrivers { get; set; }
        public string TruckType { get; set; }
        public string TruckCapacity { get; set; }
        public DateTime TruckLicenseExpiryDate { get; set; }
        public DateTime RoadWorthinessExpiryDate { get; set; }
        public DateTime InsruanceExpiryDate { get; set; }
        public List<string> Documents { get; set; }
    }
}
