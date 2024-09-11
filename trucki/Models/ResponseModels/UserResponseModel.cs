namespace trucki.Models.ResponseModels;

public class UserResponseModel
{
    public string name { get; set; }
    public string phone { get; set; }
    public bool IsActive { get; set; }
    public bool IsPasswordChanged { get; set; }
    public string email { get; set; }
}