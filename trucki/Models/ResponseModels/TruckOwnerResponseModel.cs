namespace trucki.Models.ResponseModels;

public class TruckOwnerResponseModel
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