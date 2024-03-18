using IdentityModel.Client;
using Microsoft.Extensions.Options;
using trucki.Entities;
using trucki.Interfaces.IServices;


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