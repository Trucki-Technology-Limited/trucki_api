namespace trucki.Entities;

public class TruckOwner : BaseClass
{
    public string Name { set; get; }
    public string? EmailAddress { set; get; }
    public string Phone { set; get; }
    public string Address { set; get; }
    public string IdCardUrl { set; get; }
    public string ProfilePictureUrl { set; get; }
    public string? BankDetailsId { set; get; }
    public BankDetails? BankDetails { set; get; }
    public List<Truck> trucks { set; get; }
}

