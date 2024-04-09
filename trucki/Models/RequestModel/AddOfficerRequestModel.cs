using trucki.Entities;

namespace trucki.Models.RequestModel
{
    public class AddOfficerRequestModel
    {
        public string OfficerName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public OfficerType OfficerType { get; set; }
        public string CompanyId { get; set; }
    }

    public class EditOfficerRequestModel
    {
        public string OfficerId { get; set; }
        public string OfficerName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public OfficerType OfficerType { get; set; }
        public string CompanyId { get; set; }
    }
}
