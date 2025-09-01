namespace trucki.Entities;

public class CargoOrderItem : BaseClass
{
    public string CargoOrderId { get; set; }
    public CargoOrders CargoOrder { get; set; }
    public string Description { get; set; }
    public decimal Weight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public bool IsFragile { get; set; }
    public string? SpecialHandlingInstructions { get; set; }
    public CargoType Type { get; set; }
    public int Quantity { get; set; }
    public List<string> ItemImages { get; set; } = new();


}

public enum CargoType
{
    General,
    Fragile,
    Perishable,
    Hazardous,
    Electronics,
    Furniture,
    Vehicle,
    Construction
}

public enum CargoOrderStatus
{
    Draft,
    OpenForBidding,
    BiddingInProgress,
    DriverSelected,
    DriverAcknowledged,
    ReadyForPickup,
    InTransit,
    Delivered,
    PaymentPending,
    PaymentComplete,
    PaymentOverdue,
    Cancelled
}

public class CargoOrders : BaseClass
{
    public string CargoOwnerId { get; set; }
    public CargoOwner CargoOwner { get; set; }
    public string PickupLocation { get; set; }
    public string PickupLocationLat { get; set; }
    public string PickupLocationLong { get; set; }
    public string DeliveryLocation { get; set; }
    public string DeliveryLocationLat { get; set; }
    public string DeliveryLocationLong { get; set; }
    public CargoOrderStatus Status { get; set; }
    public string CountryCode { get; set; }
    public List<CargoOrderItem> Items { get; set; } = new();
    public List<Bid> Bids { get; set; } = new();
    public string? AcceptedBidId { get; set; }
    public Bid? AcceptedBid { get; set; }
    public List<string>? Documents { get; set; } = new();
    public List<string>? DeliveryDocuments { get; set; } = new();
    public string? Consignment { get; set; }
    public DateTime? PickupDateTime { get; set; }
    public DateTime? ActualPickupDateTime { get; set; }
    public DateTime? DeliveryDateTime { get; set; }
    public CargoTruckType? RequiredTruckType { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal SystemFee { get; set; } // 20% of total amount
    public decimal Tax { get; set; }
    public string? InvoiceNumber { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime? PaymentDueDate { get; set; }
    public string? PaymentIntentId { get; set; }
    public DateTime? PaymentDate { get; set; }
    public bool IsPaid { get; set; }
    public PaymentMethodType PaymentMethod { get; set; }
    public decimal? WalletPaymentAmount { get; set; }
    public decimal? StripePaymentAmount { get; set; }
    public bool IsFlagged { get; set; } = false;
    public string? FlagReason { get; set; }
    public DateTime? FlaggedAt { get; set; }
    public string? FlaggedBy { get; set; } // Admin ID who flagged the order
    public DateTime? FlagResolvedAt { get; set; }
    public string? FlagResolvedBy { get; set; } // Admin ID who resolved the flag
    public string? FlagResolutionNotes { get; set; }
    public decimal? DriverEarnings { get; set; }

}

public enum PaymentMethodType
{
    Stripe,
    Invoice,
    Wallet,
    Mixed
}
public enum PaymentStatus
{
    Pending,
    Paid,
    Overdue
}




// First, let's create an enum for truck types
public enum CargoTruckType
{
    CargoVan,
    MiniVan,
    PickupVan,
    BoxTruck,
    FlatbedTruck,
    RefrigeratedTruck,
    DumpTruck,
    TankerTruck
}

  public enum OrderFlagType
    {
        PaymentIssue,
        DeliveryDispute,
        QualityIssue,
        CustomerComplaint,
        DriverMisconduct,
        DocumentationIssue,
        Other
    }

    public enum OrderFlagStatus
    {
        Active,
        Resolved,
        Dismissed
    }
