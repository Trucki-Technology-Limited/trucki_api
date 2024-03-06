namespace trucki.DTOs
{
    public class CreateCompanyDto
    {
        public string Name { get; set; }
        public string? Id { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string? ManagerName { get; set; }
    }
}
