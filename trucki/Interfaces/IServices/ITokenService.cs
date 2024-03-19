using IdentityModel.Client;
using trucki.DTOs;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices;

public interface ITokenService
{
    Task<TokenResponse> GetToken(string username, string password);
    Task<TokenResponse> RefreshTokenAsync(string refreshToken);
    string GenerateRefreshToken(ref ApiResponseModel<LoginResponseModel> loginResponse);

    string GenerateToken(ref ApiResponseModel<LoginResponseModel> loginResponse);
}