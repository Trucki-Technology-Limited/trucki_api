using trucki.Entities;

namespace trucki.Models.ResponseModels
{
    public class OrderCancellationResponseModel
    {
        public string OrderId { get; set; }
        public string Status { get; set; }
        public CancellationDetails CancellationDetails { get; set; }
        public RefundDetails RefundDetails { get; set; }
        public string Message { get; set; }
    }

    public class CancellationDetails
    {
        public DateTime CancelledAt { get; set; }
        public string CancellationReason { get; set; }
        public decimal PenaltyPercentage { get; set; }
        public decimal PenaltyAmount { get; set; }
        public bool RequiresApproval { get; set; }
        public string PenaltyJustification { get; set; }
    }

    public class RefundDetails
    {
        public decimal OriginalAmount { get; set; }
        public decimal PenaltyAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public PaymentMethodType OriginalPaymentMethod { get; set; }
        public trucki.Entities.RefundMethod RefundMethod { get; set; } // Use fully qualified name
        public string RefundStatus { get; set; }
        public DateTime? RefundProcessedAt { get; set; }
    }

    public class CancellationPreviewResponseModel
    {
        public string OrderId { get; set; }
        public bool CanCancel { get; set; }
        public string CancellationReason { get; set; }
        public decimal PenaltyPercentage { get; set; }
        public decimal PenaltyAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal OriginalAmount { get; set; }
        public string PenaltyJustification { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
        public bool RequiresConfirmation { get; set; }
    }
}