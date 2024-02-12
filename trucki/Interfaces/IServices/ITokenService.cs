using IdentityModel.Client;

namespace trucki.Interfaces.IServices;

public interface ITokenService
{
    Task<TokenResponse> GetToken(string username, string password);
}