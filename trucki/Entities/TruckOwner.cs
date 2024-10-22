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
}

public enum OwnersStatus
{
    Pending,

    Approved, 
    NotApproved,
    Blocked,
}
