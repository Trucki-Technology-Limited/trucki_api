using System.ComponentModel.DataAnnotations;
using trucki.Entities;

namespace trucki.DTOs
{
    public class CompanyResponseDto
    {
        public string Id { get; set; }   
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string? ManageName { get; set; }
        public ICollection<ManagerResponse> Managers { get; set; } 

    }

    public class ManagerResponse
    {
        public string? ManagerName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string CompanyId { get; set; }

        public ManagerType ManagerType { get; set; }
    }

  /*  public class GetCompanyResponse
    {
        public IEnumerable<CompanyResponseDto>? GetOrderResponse { get; set; }
    }*/
}
