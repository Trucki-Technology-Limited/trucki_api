using trucki.Entities;

namespace trucki.Models.ResponseModels;

public class CargoOrderItemResponseModel
{
    public string Id { get; set; }
    public string Description { get; set; }
    public decimal Weight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public bool IsFragile { get; set; }
    public string SpecialHandlingInstructions { get; set; }
    public CargoType Type { get; set; }
    public int Quantity { get; set; }
    public List<string> ItemImages { get; set; }
}

public class CargoOrderResponseModel
{
    public string Id { get; set; }
    public string CargoOwnerId { get; set; }
    public CargoOwnerResponseModel CargoOwner { get; set; }
    public string PickupLocation { get; set; }
    public string PickupLocationLat { get; set; }
    public string PickupLocationLong { get; set; }
    public string DeliveryLocation { get; set; }
    public string DeliveryLocationLat { get; set; }
    public string DeliveryLocationLong { get; set; }
    public CargoOrderStatus Status { get; set; }
    public List<CargoOrderItemResponseModel> Items { get; set; }
    public List<BidResponseModel> Bids { get; set; }
    public BidResponseModel AcceptedBid { get; set; }
    public DateTime? PickupDateTime { get; set; }
    public DateTime? ActualPickupDateTime { get; set; }
    public DateTime? DeliveryDateTime { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal SystemFee { get; set; }
    public decimal Tax { get; set; }
    public string InvoiceNumber { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime? PaymentDueDate { get; set; }

    public List<string>? Documents { get; set; } = new();
    public List<string>? DeliveryDocuments { get; set; } = new();

    // Order Summary Fields
    public decimal TotalWeight { get; set; }
    public decimal TotalVolume { get; set; }
    public bool HasFragileItems { get; set; }
    public Dictionary<CargoType, int> ItemTypeBreakdown { get; set; }
    public List<string> SpecialHandlingRequirements { get; set; }

    // Driver's own bid information (only set when driverId is provided)
    public DriverBidInfo? DriverBidInfo { get; set; }

    // Active Order Specific Fields
    public string NextAction { get; set; }
    public decimal AcceptedAmount { get; set; }
    public bool RequiresManifest => Status == CargoOrderStatus.ReadyForPickup && (Documents == null || !Documents.Any());
    public bool RequiresDeliveryDocuments => Status == CargoOrderStatus.InTransit &&
        (DeliveryDocuments == null || !DeliveryDocuments.Any());


    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
public class DriverBidInfo
{
    public string BidId { get; set; }
    public decimal Amount { get; set; }
    public BidStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CargoOrderSummaryModel
{
    public decimal TotalWeight { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalVolume { get; set; }
    public bool HasFragileItems { get; set; }
    public Dictionary<CargoType, int> ItemTypeBreakdown { get; set; }
    public List<string> SpecialHandlingRequirements { get; set; }
}

public class DeliveryTrackingResponseModel
{
    public string OrderId { get; set; }
    public CargoOrderStatus Status { get; set; }
    public DateTime? PickupDateTime { get; set; }
    public DateTime? ActualPickupDateTime { get; set; }
    public DateTime? DeliveryDateTime { get; set; }
    public string? CurrentLocation { get; set; }
    public string? CurrentLatitude { get; set; }
    public string? CurrentLongitude { get; set; }
    public DateTime? LastUpdateTime { get; set; }
    public DateTime? EstimatedTimeOfArrival { get; set; }
    public List<DeliveryLocationUpdate> LocationHistory { get; set; }
}