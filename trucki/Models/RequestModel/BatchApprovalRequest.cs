using System.ComponentModel.DataAnnotations;

namespace trucki.Models.RequestModel
{
    public class BatchDocumentApprovalRequest
    {
        [Required]
        public List<string> DocumentIds { get; set; } = new List<string>();

        public string? RejectionReason { get; set; }
    }
}