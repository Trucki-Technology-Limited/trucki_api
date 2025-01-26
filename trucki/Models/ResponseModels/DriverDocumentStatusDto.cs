namespace trucki.Models.ResponseModels
{
    public class DriverDocumentStatusDto
    {
        public string DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; }
        public bool IsRequired { get; set; }

        // Info about the driver's actual upload
        public bool IsUploaded { get; set; }             // True if the driver has at least one document record
        public string ApprovalStatus { get; set; }       // "Pending", "Approved", "Rejected", or "NotUploaded"
        public string FileUrl { get; set; }              // If uploaded
        public string RejectionReason { get; set; }      // If rejected
    }
}
