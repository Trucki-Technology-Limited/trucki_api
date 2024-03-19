using IdentityModel.Client;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using trucki.DTOs;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services;

public class TokenService : ITokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<IdentityServerSettings> _identityServerSettings;
        private readonly IConfiguration _config;
        //    private readonly IOptions<IdentityServerSettingsRemote> _identityServerSettingsRemote;
        private readonly HttpClient _httpClient;
        private readonly DiscoveryDocumentResponse _discoveryDocument;

        public TokenService(IOptions<IdentityServerSettings> identityServerSettings, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _identityServerSettings = identityServerSettings;
            _config = configuration;
            _httpClient = new HttpClient();
            _discoveryDocument = _httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _config.GetSection("IdentityServerSettings").GetSection("DiscoveryUrl1").Value,

                Policy =
            {
                ValidateIssuerName = false,
                RequireHttps = false,
            }
            }).Result;



            if (_discoveryDocument.IsError)
            {


                throw new Exception($"Unable to get discovery document =>{_discoveryDocument.Error}", _discoveryDocument.Exception);
            }
        }

        public async Task<TokenResponse> GetToken(string username, string password)
        {
            
            var req = new PasswordTokenRequest
            {

                Address = $"{_config.GetSection("IdentityServerSettings").GetSection("DiscoveryUrl1").Value}/connect/token",
                ClientId = "m2m",
                ClientSecret = "ClientSecret1",
                ClientCredentialStyle = ClientCredentialStyle.PostBody,
                Scope = "trucki.read trucki.write",


                UserName = username,
                Password = password

            };
            var tokenResponse = await _httpClient.RequestPasswordTokenAsync(req);

            if (tokenResponse.IsError)
            {
                throw new Exception("Unable to get token " + tokenResponse.Error, tokenResponse.Exception);
            }
            return tokenResponse;
        }


    public string GenerateRefreshToken(ref ApiResponseModel<LoginResponseModel> loginResponse)
    {
        var jwtKey = _config["jwt:key"];
        var jwtAudience = _config["jwt:audience"];
        var jwtTokenExpire = _config["jwt:expireMin"];

        DateTime issuedAt = DateTime.Now;

        //set the time when it expires
        var mins = int.TryParse(jwtTokenExpire, out int expiresMin) ? expiresMin : 0;
        DateTime expires = DateTime.Now.AddMinutes(expiresMin);


        //create a identity and add claims to the user which we want to log in
        var claims = new Claim[]
        {

                new Claim(ClaimTypes.NameIdentifier,loginResponse.Data.Id ?? ""),
                new Claim(ClaimTypes.Name, loginResponse.Data.FirstName +" "+ loginResponse.Data.LastName ?? ""),
                new Claim(ClaimTypes.GivenName,loginResponse.Data.FirstName ?? ""),
                new Claim(ClaimTypes.Surname,loginResponse.Data.LastName ?? ""),
                new Claim(ClaimTypes.MobilePhone,loginResponse.Data.PhoneNumber ?? ""),
                new Claim(ClaimTypes.Email,loginResponse.Data.EmailAddress ?? ""),
        };

        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        var token = new JwtSecurityToken(
            issuer: jwtAudience,
            audience: jwtAudience,
            claims: claims,
            notBefore: DateTime.Now,
            expires: expires,
            signingCredentials: new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256)
        );

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        return jwtToken;
    }

    public string GenerateToken(ref ApiResponseModel<LoginResponseModel> loginResponse)
    {
        var jwtKey = _config["jwt:key"];
        var jwtAudience = _config["jwt:audience"];
        var jwtTokenExpire = _config["jwt:expireMin"];

        DateTime issuedAt = DateTime.Now;

        //set the time when it expires
        var mins = int.TryParse(jwtTokenExpire, out int expiresMin) ? expiresMin : 0;
        DateTime expires = DateTime.Now.AddMinutes(expiresMin);


        //create a identity and add claims to the user which we want to log in
        var claims = new Claim[]
        {

                new Claim(ClaimTypes.NameIdentifier,loginResponse.Data.Id ?? ""),
                new Claim(ClaimTypes.Name, loginResponse.Data.FirstName +" "+ loginResponse.Data.LastName ?? ""),
                new Claim(ClaimTypes.GivenName,loginResponse.Data.FirstName ?? ""),
                new Claim(ClaimTypes.Surname,loginResponse.Data.LastName ?? ""),
                new Claim(ClaimTypes.MobilePhone,loginResponse.Data.PhoneNumber ?? ""),
                new Claim(ClaimTypes.Email,loginResponse.Data.EmailAddress ?? ""),
        };

        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        var token = new JwtSecurityToken(
            issuer: jwtAudience,
            audience: jwtAudience,
            claims: claims,
            notBefore: DateTime.Now,
            expires: expires,
            signingCredentials: new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256)
        );

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        return jwtToken;
    }



    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {

            var tokenResponse = await _httpClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {

                Address = _discoveryDocument.TokenEndpoint,
                ClientId = _identityServerSettings.Value.ClientName,
                ClientSecret = _identityServerSettings.Value.ClientPassword,
                ClientCredentialStyle = ClientCredentialStyle.PostBody,

                RefreshToken = refreshToken,



                Scope = " trucki.read trucki.write",
            });

            if (tokenResponse.IsError)
            {
                throw new Exception("Unable to get token ---" + _identityServerSettings.Value.ClientPassword + "---" + _identityServerSettings.Value.ClientName, tokenResponse.Exception);
            }

            return tokenResponse;
        }
}