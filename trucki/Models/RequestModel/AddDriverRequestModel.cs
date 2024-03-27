namespace trucki.Models.RequestModel;

public class AddDriverRequestModel
{
    public IFormFile Picture { set; get; }
    public IFormFile IdCard { set; get; }
    public string Name { set; get; }
    public string Email { set; get; }
    public string Number { set; get; }
    public string address { set; get; }
    public string? TruckId { set; get; }
}