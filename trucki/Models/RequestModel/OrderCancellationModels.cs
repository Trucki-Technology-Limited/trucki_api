using System.ComponentModel.DataAnnotations;

namespace trucki.Models.RequestModel
{
 public class CancelOrderRequestDto
    {
        [Required]
        public string OrderId { get; set; }
        
        [Required]
        public string CargoOwnerId { get; set; }
        
        [MaxLength(500)]
        public string? CancellationReason { get; set; }
        
        public bool AcknowledgePenalty { get; set; } = false;
    }
    
    public class ProcessCancellationRefundDto
    {
        [Required]
        public string OrderId { get; set; }
        
        [Required]
        public string AdminId { get; set; }
        
        public string? AdminNotes { get; set; }
    }
}