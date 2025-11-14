using System.ComponentModel.DataAnnotations.Schema;

namespace trucki.Entities;

public class TruckOwner : BaseClass
{
    public string Name { set; get; }
    public string? EmailAddress { set; get; }
    public string Phone { set; get; }
    public string Address { set; get; }
    public string? IdCardUrl { set; get; }
    public string? ProfilePictureUrl { set; get; }
    public string? BankDetailsId { set; get; }
    public BankDetails? BankDetails { set; get; }
    public List<Truck> trucks { set; get; }
    public List<Driver> drivers { set; get; }
     [ForeignKey("User")]
    public string? UserId { get; set; }

    public User? User { get; set; }
    public OwnersStatus OwnersStatus { get; set; }

    // New properties for dispatcher functionality
    public TruckOwnerType OwnerType { get; set; } = TruckOwnerType.TruckOwner;
    public string Country { get; set; } = "NG"; // Country code (NG, US, etc.)
    public bool CanBidOnBehalf { get; set; } = false; // For dispatchers

    // DOT and MC Numbers for dispatchers (optional)
    // If dispatcher doesn't provide these, their drivers MUST provide them
    public string? DotNumber { get; set; }
    public string? McNumber { get; set; }

    // Navigation property for dispatcher commission structures
    public ICollection<DriverDispatcherCommission> DispatcherCommissions { get; set; } = new List<DriverDispatcherCommission>();
}

public enum TruckOwnerType
{
    TruckOwner,      // Traditional truck owner
    Transporter,     // Nigerian market transporter
    Dispatcher       // US market dispatcher
}

public enum OwnersStatus
{
    Pending,

    Approved,
    NotApproved,
    Blocked,
}
