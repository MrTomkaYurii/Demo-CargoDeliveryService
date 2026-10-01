namespace Drivers.Api.Helpers
{
    public class jwtConfig
    {
        public string Secret { get; set; }
        public string ValidIssuer { get; set; }
        public string ValidAudience { get; set; }
    }
}
