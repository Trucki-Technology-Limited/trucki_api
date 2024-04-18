using trucki.Entities;

namespace trucki.Models.RequestModel
{
    public class CreateOrderRequestModel
    {
        public string TruckNo { get; set; }
        public string Quantity { get; set; }
        public string RouteFrom { get; set; }
        public string RouteTo { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public OrderStatus OrderStatus { get; set; }

        // Field officer ID to be attached to the order
        public string FieldOfficerId { get; set; }
    }

    public class EditOrderRequestModel
    {
        public string OrderId { get; set; }
        public string TruckNo { get; set; }
        public string Quantity { get; set; }
        public string RouteFrom { get; set; }
        public string RouteTo { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
}
