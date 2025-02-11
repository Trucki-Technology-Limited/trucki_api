namespace trucki.Entities
{
    public class Invoice : BaseClass
    {
        public string OrderId { get; set; }
        public CargoOrders Order { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal SubTotal { get; set; }
        public decimal SystemFee { get; set; } // 20% of subtotal
        public decimal Tax { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime DueDate { get; set; }
        public InvoiceStatus Status { get; set; }
        public string? PaymentProofUrl { get; set; }
        public DateTime? PaymentSubmittedAt { get; set; }
        public string? PaymentApprovedBy { get; set; }
        public DateTime? PaymentApprovedAt { get; set; }
        public string? PaymentNotes { get; set; }
    }

    public enum InvoiceStatus
    {
        Pending,
        PaymentSubmitted,
        PaymentApproved,
        Overdue,
        Paid
    }

    public class PaymentAccount : BaseClass
    {
        public string BankName { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string RoutingNumber { get; set; }
        public string SwiftCode { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
    }
}