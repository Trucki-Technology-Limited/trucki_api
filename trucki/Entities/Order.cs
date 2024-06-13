namespace trucki.Entities
{
    public class Order : BaseClass
    {
        public string OrderId { get; set; }
        public string? TruckNo { get; set; }
        public string Quantity { get; set; }
        public string CargoType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public OrderStatus OrderStatus { get; set; }
        public float? Price { get; set; }

        //  Foreign key property referencing the Manager and Officer class
        public string? TruckId { get; set; }
        public Truck? Truck { get; set; }
        public string? BusinessId { get; set; }
        public Business? Business { get; set; } 
        public string? RoutesId { get; set; }
        public Routes? Routes { get; set; }    
        public string OfficerId { get; set; }
        public Officer Officer { get; set; }
        public string? ManagerId { get; set; }
        public Manager? Manager { get; set; }
        public string? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public string? DeliveryAddress { get; set; }
        public bool is60Paid { get; set; } = false;
        public bool is40Paid { get; set; } = false;
        public List<string>? Documents { get; set; } = new(); 
        public List<string>? DeliveryDocuments { get; set; } = new(); 
    }

    public enum OrderStatus
    {
        Pending,
        Assigned,
        Loaded,
        InTransit,
        Destination,
        Delivered,
        Flagged,
        Archived

    }
}
