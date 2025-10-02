namespace trucki.Models.ResponseModels
{
    public class DriverDocumentResponseDto
    {
        public string Id { get; set; }
        public string DriverId { get; set; }
        public string DocumentTypeId { get; set; }
        public string FileUrl { get; set; }
        public string ApprovalStatus { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}