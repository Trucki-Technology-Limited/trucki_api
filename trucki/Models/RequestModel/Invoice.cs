
using trucki.Entities;

namespace trucki.Models.RequestModel
{
    public class SubmitPaymentProofDto
    {
        public string InvoiceId { get; set; }
        public string PaymentProofUrl { get; set; }
        public string? Notes { get; set; }
    }

    public class ApprovePaymentDto
    {
        public string InvoiceId { get; set; }
        public string AdminId { get; set; }
        public string? Notes { get; set; }
    }

    public class GetInvoicesQueryDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public InvoiceStatus? Status { get; set; }
        public string? SortBy { get; set; } // DueDate, Amount, Status
        public bool SortDescending { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}