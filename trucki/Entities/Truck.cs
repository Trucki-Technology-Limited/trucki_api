namespace trucki.Entities
{
    public class Truck : BaseClass
    {
        public string? TruckOwner { get; set; }
        //To be change to enum
        public string? TruckType { get; set; }
        public string? PlateNumebr { get; set; }
        public string? Capacity { get; set; }
        //To be change to enum
        public string? DriverType { get; set; }

        public string Status { get; set; }

        public string? HealthRate { get; set; }

        public DateTime TruckLicenceExpiryDate { get; set; }
        public DateTime RoadWorthinessExpiryDate { get; set; }
        public DateTime InsuranceExpiryDate { get; set; }

    }
}
