using Duende.IdentityServer.Models;

namespace trucki.CustomExtension;

 public static class IdentityServerConfiguration
    {
        // IdentityResources: what are to be protected with IdentityServer

        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Email(),
                new IdentityResources.Profile(),

                new IdentityResource
                {
                    Name = "role",
                    UserClaims = new List<string> {"role"}
                }
            };

        // scope is an Identifier for the resource that the client want to access
        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope(name: "trucki.read"),
                new ApiScope(name: "trucki.write")

            };


        public static IEnumerable<ApiResource> ApiResources =>
          new List<ApiResource>
          {
                new ApiResource(name: "trucki")
                {
                    Scopes = new List<string> { "trucki.read", "trucki.write"},
                    ApiSecrets = new List<Secret> {new Secret ("ScopeSecret".Sha256())},
                    UserClaims = new List<string> {"role"}
                },
             
          };

        // Client: a piece of software that request a token from the IdentityServer
        public static IEnumerable<Client> Clients =>
            new List<Client>
            {

                new Client
                {
                    ClientId = "m2m",
                    ClientName = "trucki.client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets = { new Secret("ClientSecret1".Sha256()) },
                    AllowedScopes =
                    {
                        "trucki.read", "trucki.write"
                    },
                    AllowOfflineAccess = true,

                    AccessTokenType = AccessTokenType.Jwt,

                    AccessTokenLifetime = 86400,
                    IdentityTokenLifetime = 120,
                    UpdateAccessTokenClaimsOnRefresh = true,

                    RefreshTokenExpiration = TokenExpiration.Absolute,
                    RefreshTokenUsage = TokenUsage.ReUse,
                    AbsoluteRefreshTokenLifetime = 360000,
                    Enabled = true,
                },
            };
    }