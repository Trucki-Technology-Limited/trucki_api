using trucki.Entities;

namespace trucki.Models.ResponseModels
{
    public class InvoiceResponseModel
    {
        public string Id { get; set; }
        public string InvoiceNumber { get; set; }
        public string OrderId { get; set; }
        public InvoiceCargoOrderSummaryModel Order { get; set; }
        public decimal SubTotal { get; set; }
        public decimal SystemFee { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime DueDate { get; set; }
        public InvoiceStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? PaymentProofUrl { get; set; }
        public DateTime? PaymentSubmittedAt { get; set; }
        public string? PaymentNotes { get; set; }
        public bool IsOverdue => Status != InvoiceStatus.Paid && DueDate < DateTime.UtcNow;
    }

    public class InvoiceCargoOrderSummaryModel
    {
        public string Id { get; set; }
        public string PickupLocation { get; set; }
        public string DeliveryLocation { get; set; }
        public DateTime? DeliveryDateTime { get; set; }
        public decimal TotalWeight { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalVolume { get; set; }
        public bool HasFragileItems { get; set; }
        public Dictionary<CargoType, int> ItemTypeBreakdown { get; set; }
        public List<string> SpecialHandlingRequirements { get; set; }
    }

    public class BankAccountResponseModel
    {
        public string BankName { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string RoutingNumber { get; set; }
        public string SwiftCode { get; set; }
        public string? Notes { get; set; }
    }
}