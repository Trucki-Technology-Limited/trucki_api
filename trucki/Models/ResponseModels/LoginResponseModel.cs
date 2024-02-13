namespace trucki.Models.ResponseModels;

public class LoginResponseModel
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
}