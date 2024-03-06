

namespace trucki.Shared.Request
{
    public class GetRequest
    {
        public string Url { get; set; }
    }

    public class AuthGetRequest
    {
        public string Url { get; set; }
        public string AuthToken { get; set; }
    }
}
