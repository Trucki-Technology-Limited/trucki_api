using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices;

public interface IAuthService
{
    Task<ApiResponseModel<LoginResponseModel>> Login(LoginRequestModel request);
    Task<ApiResponseModel<bool>> AddNewUserAsync(string name, string email, string phone, string role, string password, bool hasChangedPassword);
    Task<ApiResponseModel<UserResponseModel>> GetUserById(string userId);
    Task<ApiResponseModel<bool>> ChangePasswordAsync(string userId, string newPassword);
    Task<ApiResponseModel<bool>> UpdateUserPassword(string userId, UpdateUsersPasswordRequestModel requestModel);
    Task<ApiResponseModel<bool>> ForgotPasswordAsync(string email);
    Task<ApiResponseModel<bool>> VerifyResetCodeAsync(string email, string resetCode);
    Task<ApiResponseModel<bool>> ResetPasswordAsync(string email, string resetCode, string newPassword);
    Task<ApiResponseModel<bool>> RegisterDeviceTokenAsync(string userId, string token, string deviceType);
}