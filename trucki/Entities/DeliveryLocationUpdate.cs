namespace trucki.Entities
{
    public class DeliveryLocationUpdate : BaseClass
    {
        public string OrderId { get; set; }
        public CargoOrders Order { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string CurrentLocation { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime? EstimatedTimeOfArrival { get; set; }
    }
}