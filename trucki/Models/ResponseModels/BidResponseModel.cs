
namespace trucki.Models.ResponseModels;
public class BidResponseModel
{
    public string Id { get; set; }
    public string OrderId { get; set; }
    public string TruckId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public AllTruckResponseModel Truck { get; set; }
    public DriverProfileResponseModel Driver { get; set; }
}