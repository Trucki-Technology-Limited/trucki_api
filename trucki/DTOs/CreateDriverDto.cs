using System.ComponentModel.DataAnnotations;

namespace trucki.DTOs
{
    public class CreateDriverDto
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string?   EmailAddress { get; set; }

        public string Id { get; set; }

        public string? Address { get; set; }

        public string? DriverExperience { get; set; }

        public string? Company { get; set; }

        public byte[]? DriversLicence { get; set; }

        public byte[]? PassportFile { get; set; }
    }
}
