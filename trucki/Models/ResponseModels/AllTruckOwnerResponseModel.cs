using trucki.Entities;

namespace trucki.Models.ResponseModels;

public class AllTruckOwnerResponseModel
{
    public string Id { get; set; }
    public string Name { set; get; }
    public string? EmailAddress { set; get; }
    public string Phone { set; get; }
    public string Address { set; get; }
    public string IdCardUrl { set; get; }
    
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}


public class TruckOwnerResponseModel
{
    public string Id { get; set; }
    public string Name { set; get; }
    public string? EmailAddress { set; get; }
    public string Phone { set; get; }
    public string Address { set; get; }
    public string IdCardUrl { set; get; }
    public string ProfilePictureUrl { set; get; }
    public string noOfTrucks { set; get; }

    public bool IsProfileSetupComplete { set; get; }
    public bool IsAccountApproved { set; get; }
    public BankDetailsResponseModel BankDetails { set; get; }
    
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    public OwnersStatus OwnersStatus { get; set; }  
}