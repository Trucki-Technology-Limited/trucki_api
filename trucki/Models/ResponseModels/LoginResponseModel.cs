namespace trucki.Models.ResponseModels;

public class LoginResponseModel
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public IEnumerable<string> Role { get; set; }
    public string EmailAddress { get; set; }
    public List<string> Permissions { get; set; }
    public bool isPasswordChanged { get; set; }
    public bool isEmailConfirmed { get; set; }
    public bool isPhoneNumberConfirmed { get; set; }
    public DateTime? LastLoginDate { get; set; }
}


/*public string Id { get; set; }
public string UserName { get; set; }
public string Token { get; set; }
public string RefreshToken { get; set; }*/