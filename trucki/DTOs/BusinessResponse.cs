using trucki.Entities;

namespace trucki.DTOs
{
    public class BusinessResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Ntons { get; set; }
        public string Address { get; set; }
        public List<RoutesResponse>? Routes { get; set; }
    }


    public class RoutesResponse
    {
        public string Name { get; set; }

        public string? BusinessId { get; set; }

        public string FromRoute { get; set; }

        public string ToRoute { get; set; }

        public float Price { get; set; }

        public bool IsActive { get; set; }
    }
}
