using trucki.Entities;
using System.ComponentModel.DataAnnotations;

namespace trucki.Models.RequestModel
{
    /// <summary>
    /// Request model for updating truck approval status
    /// </summary>
    public class UpdateApprovalStatusRequest
    {
        /// <summary>
        /// New approval status for the truck
        /// </summary>
        [Required]
        public ApprovalStatus ApprovalStatus { get; set; }
        
        /// <summary>
        /// Optional reason for rejection (required when status is NotApproved or Blocked)
        /// </summary>
        public string? RejectionReason { get; set; }
        
        /// <summary>
        /// Admin notes for the approval decision
        /// </summary>
        public string? AdminNotes { get; set; }
    }
}