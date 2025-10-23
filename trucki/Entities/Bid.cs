namespace trucki.Entities;

public class Bid : BaseClass
{
    public string OrderId { get; set; }
    public CargoOrders Order { get; set; }
    public string TruckId { get; set; }
    public Truck Truck { get; set; }
    public decimal Amount { get; set; }
    public BidStatus Status { get; set; }
    public DateTime? DriverAcknowledgedAt { get; set; }

    // Enhanced bidding properties for dispatcher functionality
    public BidSubmitterType SubmitterType { get; set; } = BidSubmitterType.Driver;
    public string? SubmittedByDispatcherId { get; set; } // If bid by dispatcher
    public TruckOwner? SubmittedByDispatcher { get; set; }
    public decimal? DispatcherCommissionAmount { get; set; } // Calculated commission
    public string? Notes { get; set; }

    // Important: The bid always references the actual driver/truck
    // CargoOwner sees only the driver, Admin sees both dispatcher and driver
}


public enum BidSubmitterType
{
    Driver,      // Direct driver bid
    Dispatcher   // Bid submitted by dispatcher on behalf of driver
}

public enum BidStatus
{
    Pending,
    AdminApproved,
    AdminRejected,
    CargoOwnerSelected,
    DriverAcknowledged,
    DriverDeclined,
    Expired
}