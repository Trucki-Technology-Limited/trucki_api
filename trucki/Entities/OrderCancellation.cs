using System.ComponentModel.DataAnnotations.Schema;

namespace trucki.Entities
{
    public class OrderCancellation : BaseClass
    {
        public string OrderId { get; set; }
        
        [ForeignKey("OrderId")]
        public CargoOrders Order { get; set; }
        
        public string CargoOwnerId { get; set; }
        
        [ForeignKey("CargoOwnerId")]
        public CargoOwner CargoOwner { get; set; }
        
        public string? CancellationReason { get; set; }
        
        public CargoOrderStatus OrderStatusAtCancellation { get; set; }
        
        public decimal PenaltyPercentage { get; set; }
        
        public decimal PenaltyAmount { get; set; }
        
        public decimal OriginalAmount { get; set; }
        
        public decimal RefundAmount { get; set; }
        
        public PaymentMethodType? OriginalPaymentMethod { get; set; }
        
        public RefundMethod RefundMethod { get; set; }
        
        public CancellationStatus Status { get; set; }
        
        public DateTime CancelledAt { get; set; }
        
        public DateTime? RefundProcessedAt { get; set; }
        
        public string? ProcessedBy { get; set; }  // Admin who processed the refund
        
        public string? AdminNotes { get; set; }
        
        public bool IsDriverNotified { get; set; } = false;
        
        public string? DriverId { get; set; }  // Driver who was assigned (if any)
        
        public string? RefundTransactionId { get; set; }  // External reference for refund transaction
    }
    
    public enum CancellationStatus
    {
        Requested,
        Approved,
        RefundPending,      // Automatic refund failed, needs admin attention
        RefundProcessed,
        Completed,
        Rejected
    }
    
    public enum RefundMethod
    {
        Wallet,
        InvoiceVoid,
        ManualBankTransfer,
        NoneRequired
    }
}