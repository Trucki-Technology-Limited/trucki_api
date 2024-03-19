using trucki.Entities;

namespace trucki.DTOs
{
    public class CreateBusinessDto
    {
        public string Name { get; set; }
        public string Ntons { get; set; }
        public string Address { get; set; }
        public bool isActive { get; set; }
    }
}
