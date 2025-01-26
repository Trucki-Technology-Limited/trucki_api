using System.ComponentModel.DataAnnotations.Schema;

namespace trucki.Entities
{
    public class DriverDocument : BaseClass
    {
        // Foreign key to Driver
        public string DriverId { get; set; }   // Assuming your BaseClass uses string Id or int, adapt accordingly
        public Driver Driver { get; set; }

        // Foreign key to DocumentType
        public string DocumentTypeId { get; set; }
        public DocumentType DocumentType { get; set; }

        // Where the document file is stored (S3, local path, etc.)
        public string FileUrl { get; set; }

        // E.g., "Pending", "Approved", "Rejected"
        public string ApprovalStatus { get; set; } = "Pending";

        // Reason for rejection
        public string? RejectionReason { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
    }
}
