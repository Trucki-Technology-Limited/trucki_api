namespace trucki.Entities
{
    public class Order : BaseClass
    {
        public string TruckNo { get; set; }
        public string Quantity { get; set; }
        public string RouteFrom { get; set; }
        public string RouteTo { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public OrderStatus OrderStatus { get; set; }

        //  Foreign key property referencing the Officer class
        public string FieldOfficerId { get; set; }
        public Officer Officer { get; set; }
    }

    public enum OrderStatus
    {
        Pending,
        InTransit,
        Loaded,
        Destination,
        Delivered,
        Flagged,
        Archived

    }
}
