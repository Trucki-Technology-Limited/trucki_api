using trucki.Entities;

namespace trucki.Models.ResponseModels
{
    public class AccountDeletionResponseModel
    {
        public string Id { get; set; }
        public string UserType { get; set; }
        public string DeletionReason { get; set; }
        public string Country { get; set; }
        public AccountDeletionStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? AdminNotes { get; set; }
    }
}