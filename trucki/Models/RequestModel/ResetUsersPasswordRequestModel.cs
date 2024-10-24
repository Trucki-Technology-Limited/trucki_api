namespace trucki.Models.RequestModel;

public class ResetUsersPasswordRequestModel
{
    public string userId { get; set; }
    public string password { get; set; }
}

public class UpdateUsersPasswordRequestModel
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmNewPassword { get; set; }
}