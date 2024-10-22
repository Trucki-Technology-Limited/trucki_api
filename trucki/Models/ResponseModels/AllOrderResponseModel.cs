using trucki.Entities;

namespace trucki.Models.ResponseModels
{
    public class AllOrderResponseModel
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string TruckNo { get; set; }
        public string Quantity { get; set; }
        public string DeliveryAddress { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public RouteResponseModel? Routes { get; set; }
        public AllCustomerResponseModel Customer { get; set; }
        public AllBusinessResponseModel? Business { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class OrderResponseModel
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string businessId { get; set; }
        public string TruckNo { get; set; }
        public string Quantity { get; set; }
        public string CargoType { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string RouteFrom { get; set; }
        public string RouteTo { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public float? Price {  get; set; }
        public string Driver { get; set; }
        
        public string Customer { get; set; }
        
        public string DeliveryLocation { get; set; }
        public List<string> Documents { get; set; } 
        public List<string> DeliveryDocuments { get; set; } 
        public string?  TruckOwnerName{ get; set; }
        public string?  TruckOwnerBankName { get; set; }
        public string?  TruckOwnerBankAccountNumber { get; set; }
        public bool is60Paid { get; set; } 
        public bool is40Paid { get; set; }
        public DateTime CreatedAt { get; set; }

    }
    public class OrderStatsResponse
    {
        public int CompletedOrders { get; set; }
        public int FlaggedOrders { get; set; }
        public int InTransitOrders { get; set; }
        public int TotalOrders { get; set;}
    }
    
    public class OrderResponseModelForMobile
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string businessId { get; set; }
        public string businessName { get; set; }
        public string businessLocation { get; set; }
        public string customerId { get; set; }
        public string customerName { get; set; }
        public string customerLocation { get; set; }
        public string TruckNo { get; set; }
        public string Quantity { get; set; }
        public string CargoType { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string RouteFrom { get; set; }
        public string RouteTo { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public float? Price {  get; set; }
        public string Driver { get; set; }
        
        public string Customer { get; set; }
        
        public string DeliveryLocation { get; set; }
        public List<string> Documents { get; set; } 
        public List<string> DeliveryDocuments { get; set; } 
        public string?  TruckOwnerName{ get; set; }
        public string?  TruckOwnerBankName { get; set; }
        public string?  TruckOwnerBankAccountNumber { get; set; }
        public bool is60Paid { get; set; } 
        public bool is40Paid { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
