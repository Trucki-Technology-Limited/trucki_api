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
        public string CompanyId { get; set; }
    }
}
