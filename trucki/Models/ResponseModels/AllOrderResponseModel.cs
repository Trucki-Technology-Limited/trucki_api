using trucki.Entities;

namespace trucki.Models.ResponseModels
{
    public class AllOrderResponseModel
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string TruckNo { get; set; }
        public string Quantity { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public AllOfficerResponseModel? FieldOfficer { get; set; }
        public RouteResponseModel? Route { get; set; }
        public AllBusinessResponseModel? Business { get; set; }
    }

    public class OrderResponseModel
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string TruckNo { get; set; }
        public string Quantity { get; set; }
        public string CargoType { get; set; }
        //public string StartDate { get; set; }
        //public string EndDate { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string FieldOfficerName { get; set; }
        public string RouteFrom { get; set; }
        public string RouteTo { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public float? Price {  get; set; }
        public string Driver { get; set; }
    }
}
