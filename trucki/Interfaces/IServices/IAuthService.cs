using trucki.DTOs;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices;

public interface IAuthService
{
    Task<ApiResponseModel<LoginResponseModel>> Login(LoginRequestModel request);
    Task<ApiResponseModel<CreatTruckiUserResponseDto>> RegisterTruckiAsync(CreatTruckiUserDto registrationRequest);
    Task<ApiResponseModel<RefreshTokenResponseDto>> RefreshToken(string refreshToken);
    Task<ApiResponseModel<string>> VerifyUser(string email);
    Task<ApiResponseModel<string>> ForgotPassword(string email);
    Task<ApiResponseModel<string>> ChangePassword(ChangePasswordDto request);
}