using System.ComponentModel.DataAnnotations;

namespace trucki.Models.RequestModel
{
    public class CancelOrderRequestDto
    {
        [Required]
        public string OrderId { get; set; }

        [Required]
        public string CargoOwnerId { get; set; }

        [Required]
        public string CancellationReason { get; set; }

        // For shippers - indicates they acknowledge the penalty
        public bool AcknowledgePenalty { get; set; } = false;

        // For brokers - payment intent ID for cancellation fee payment
        public string? CancellationFeePaymentIntentId { get; set; }
    }

    public class ProcessCancellationRefundDto
    {
        [Required]
        public string OrderId { get; set; }

        [Required]
        public string AdminId { get; set; }

        public string? AdminNotes { get; set; }
    }

    // New DTO for creating cancellation fee payment intent (for brokers)
    public class CreateCancellationFeePaymentIntentDto
    {
        [Required]
        public string OrderId { get; set; }

        [Required]
        public string CargoOwnerId { get; set; }

        [Required]
        public decimal PenaltyAmount { get; set; }

        public string Currency { get; set; } = "usd";
    }

    // DTO for confirming cancellation fee payment (for brokers)
    public class ConfirmCancellationFeePaymentDto
    {
        [Required]
        public string OrderId { get; set; }

        [Required]
        public string PaymentIntentId { get; set; }

        [Required]
        public string CargoOwnerId { get; set; }

        [Required]
        public string CancellationReason { get; set; }
    }
}