using System.ComponentModel.DataAnnotations;

namespace trucki.Models.RequestModel
{
    public class AccountDeletionRequestModel
    {
        [Required(ErrorMessage = "Deletion reason is required")]
        [StringLength(500, ErrorMessage = "Deletion reason cannot exceed 500 characters")]
        [MinLength(10, ErrorMessage = "Please provide a detailed reason (minimum 10 characters)")]
        public string DeletionReason { get; set; }
    }
}