using trucki.Entities;

namespace trucki.Models.RequestModel;

public class AcceptOrderRequestModel
{
    public string orderId { get; set; }
    public string driverId { get; set; }
    public string status { get; set; }
}
public class AssignOrderToTruckAsTransporter
{
    public string OrderId { get; set; }
    public string TruckId { get; set; }
    // Possibly the requesting truck owner userId or truckOwnerId
    public string TruckOwnerId { get; set; }
}

 public class SearchOrderRequestModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? TruckNo { get; set; }
        public OrderStatus? Status { get; set; }
        public string? Quantity { get; set; }
        public DateTime? CreatedAt { get; set; }
    }