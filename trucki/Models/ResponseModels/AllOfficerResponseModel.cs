using trucki.Entities;

namespace trucki.Models.ResponseModels
{
    public class AllOfficerResponseModel
    {
        public string Id { get; set; }
        public string OfficerName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string UserId { get; set; }
        public OfficerType OfficerType { get; set; }
        public OfficerBusinessResponseModel Company { get; set; }
    }
    public class OfficerBusinessResponseModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Address { get; set; }
        public bool isActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
