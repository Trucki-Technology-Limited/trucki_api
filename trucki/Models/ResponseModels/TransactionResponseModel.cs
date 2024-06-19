using trucki.Entities;

namespace trucki.Models.ResponseModels;

public class TransactionResponseModel
{
    public string TransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }

    // Order Details
    public string OrderId { get; set; }
    public string CargoType { get; set; } 
    public string Quantity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public OrderStatus OrderStatus { get; set; }

    // Business Details
    public string BusinessId { get; set; }
    public string BusinessName { get; set; }
    public string truckOwner { get; set; }

    // Truck Details (Optional, include if relevant)
    public string? TruckId { get; set; }
    public string? TruckNo { get; set; }
}