using trucki.Entities;

namespace trucki.Models.ResponseModels
{
    public class AllOrderResponseModel
    {
        public string Id { get; set; }
        public string TruckNo { get; set; }
        public string Quantity { get; set; }
        public string RouteFrom { get; set; }
        public string RouteTo { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string FieldOfficerId { get; set; }
    }
}
