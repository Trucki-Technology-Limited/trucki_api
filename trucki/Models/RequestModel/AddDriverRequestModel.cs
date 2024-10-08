namespace trucki.Models.RequestModel;

public class AddDriverRequestModel
{
    public string Picture { set; get; }
    public string IdCard { set; get; }
    public string Name { set; get; }
    public string Email { set; get; }
    public string Number { set; get; }
    public string address { set; get; }
    public string? TruckId { set; get; }
    public string? TruckOwnerId { get; set; }  // Add this property
}

public class EditDriverRequestModel 
{
    public string Id { get; set; }
    public string Name { set; get; }
    public string Number { get; set; }
    public String? ProfilePicture { get; set; }
}

public class CreateDriverRequestModel
{
    public string Name { set; get; }
    public string Email { set; get; }
    public string Number { set; get; }
    public string address { set; get; }
    public string password { set; get; }
}