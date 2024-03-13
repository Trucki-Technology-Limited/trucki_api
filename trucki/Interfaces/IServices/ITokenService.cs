using IdentityModel.Client;
using trucki.DTOs;

namespace trucki.Interfaces.IServices;

public interface ITokenService
{
    Task<TokenResponse> GetToken(string username, string password);
    Task<TokenResponse> RefreshTokenAsync(RefreshTokenDto refreshToken);
}