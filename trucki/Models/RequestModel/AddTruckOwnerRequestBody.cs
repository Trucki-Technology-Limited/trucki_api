namespace trucki.Models.RequestModel;

public class AddTruckOwnerRequestBody
{
    public string Name { set; get; }
    public string? EmailAddress { set; get; }
    public string Phone { set; get; }
    public string Address { set; get; }
    public IFormFile IdCard  { set; get; }
    public IFormFile ProfilePicture { get; set; }
}

public class EditTruckOwnerRequestBody
{
    public string Id { set; get; }
    public string Name { set; get; }
    public string? EmailAddress { set; get; }
    public string Phone { set; get; }
    public string Address { set; get; }
    public IFormFile? IdCard  { set; get; }
}