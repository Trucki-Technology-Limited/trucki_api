using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices;

public interface IAuthService
{
    Task<ApiResponseModel<LoginResponseModel>> Login(LoginRequestModel request);
    Task<ApiResponseModel<bool>> AddNewUserAsync(string name, string email,string phone, string role, string password);
    Task<ApiResponseModel<UserResponseModel>> GetUserById(string userId);
    Task<ApiResponseModel<bool>> ChangePasswordAsync(string userId, string newPassword);
}