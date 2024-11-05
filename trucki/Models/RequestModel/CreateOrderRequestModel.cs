using trucki.Entities;

namespace trucki.Models.RequestModel
{
    public class CreateOrderRequestModel
    {
        public string Quantity { get; set; }
        public string CargoType { get; set; }
        
        public string CompanyId { get; set; }
        public string CustomerId { get; set; }
        public string RouteId { get; set; }
        public string StartDate { get; set; }
        public string DeliveryAddress { get; set; }
        public string DeliveryLocationLat { get; set; }
        public string DeliveryLocationLong { get; set; }
        public string Consignment { get; set; }
    }

    public class AssignTruckRequestModel
    {
        public string OrderId { get; set; }
        public string TruckId { get; set; }
    }
    public class EditOrderRequestModel
    {
        public string Id { get; set; }
        public string Quantity { get; set; }
        public string CargoType { get; set; }
        //public string StartDate { get; set; }
        public OrderStatus OrderStatus { get; set; }
    }
}
