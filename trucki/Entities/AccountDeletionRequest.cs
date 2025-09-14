using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace trucki.Entities
{
    public class AccountDeletionRequest : BaseClass
    {
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserType { get; set; } // "Driver" or "CargoOwner"

        [Required]
        [MaxLength(500)]
        public string DeletionReason { get; set; }

        [Required]
        [MaxLength(10)]
        public string Country { get; set; } // Must be "US" for this endpoint

        public AccountDeletionStatus Status { get; set; } = AccountDeletionStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        [MaxLength(500)]
        public string? AdminNotes { get; set; }

        public string? ProcessedByUserId { get; set; }
    }

    public enum AccountDeletionStatus
    {
        Pending,
        Approved,
        Rejected,
        Completed
    }
}