namespace trucki.Models.ResponseModels;

public class CargoOwnerProfileResponseModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string EmailAddress { get; set; }
    public string CompanyName { get; set; }
    public string Address { get; set; }
    public string Country { get; set; }
    public bool IsActive { get; set; }
}