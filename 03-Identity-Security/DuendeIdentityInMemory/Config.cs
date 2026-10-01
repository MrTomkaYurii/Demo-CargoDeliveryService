using Duende.IdentityServer.Models;

namespace DuendeIdentityInMemory
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("DriverApi.read"),
                new ApiScope("DriverApi.write"),
            };

        public static IEnumerable<ApiResource> ApiResources => new[]
        {
            new ApiResource("DriverApi")
            {
                Scopes = new List<string> { "DriverApi.read", "DriverApi.write"},
            }
        };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                // m2m client credentials flow client
                new Client
                {
                    ClientId = "m2m.client",
                    ClientName = "Client Credentials Client",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                    AllowedScopes = { "DriverApi.read", "DriverApi.write" }
                },

                // interactive client using code flow + pkce
                new Client
                {
                    ClientId = "swagger",
                    ClientName = "Swagger UI for Sample API",
                    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,

                    RequirePkce = true,
                    RedirectUris = { "https://localhost:4002/swagger/oauth2-redirect.html" },
                    AllowedCorsOrigins = {"https://localhost:4002"},

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "DriverApi.read", "DriverApi.write" }
                },

            };
    }
}
