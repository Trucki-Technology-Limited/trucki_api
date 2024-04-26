using trucki.Entities;

namespace trucki.Models.RequestModel
{
    public class CreateOrderRequestModel
    {
        public string ManagerId { get; set; }
        //public string TruckNo { get; set; }
        public string Quantity { get; set; }
        public string CargoType { get; set; }
        //public string StartDate { get; set; }
        //public OrderStatus OrderStatus { get; set; }

        // Field officer ID to be attached to the order
        public string FieldOfficerId { get; set; }
    }

    public class AssignTruckRequestModel
    {
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public string TruckId { get; set; }
        public string RouteId { get; set; }
        public decimal Price { get; set; }
        public string StartDate { get; set; }
    }
    public class EditOrderRequestModel
    {
        public string Id { get; set; }
        public string Quantity { get; set; }
        public string CargoType { get; set; }
        //public string StartDate { get; set; }
        public OrderStatus OrderStatus { get; set; }

        // Field officer ID to be attached to the order
        public string FieldOfficerId { get; set; }
    }
}
