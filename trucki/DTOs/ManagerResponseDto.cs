using System.ComponentModel.DataAnnotations;
using trucki.Entities;

namespace trucki.DTOs
{
    public class ManagerResponseDto
    {
        public string? Id { get; set; }
        public string? ManagerName { get; set; }

        public string PhoneNumber { get; set; }

        public string EmailAddress { get; set; }

        public string CompanyId { get; set; }
        public ManagerType ManagerType { get; set; }
    }
}
