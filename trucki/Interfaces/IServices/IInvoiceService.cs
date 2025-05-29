using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Interfaces.IServices
{
    public interface IInvoiceService
    {
        /// <summary>
        /// Generates a new invoice for a delivered order
        /// </summary>
        Task<ApiResponseModel<InvoiceResponseModel>> GenerateInvoiceAsync(string orderId);

        /// <summary>
        /// Gets paginated list of invoices for a cargo owner with filtering and sorting
        /// </summary>
        Task<ApiResponseModel<PagedResponse<InvoiceResponseModel>>> GetCargoOwnerInvoicesAsync(
            GetInvoicesQueryDto query);

        /// <summary>
        /// Submit payment proof for an invoice
        /// </summary>
        Task<ApiResponseModel<bool>> SubmitPaymentProofAsync(SubmitPaymentProofDto submitPaymentDto);

        /// <summary>
        /// Admin approval of payment proof
        /// </summary>
        Task<ApiResponseModel<bool>> ApprovePaymentAsync(ApprovePaymentDto approvePaymentDto);

        /// <summary>
        /// Get active bank account details for payment
        /// </summary>
        Task<ApiResponseModel<BankAccountResponseModel>> GetActiveBankAccountAsync();

        /// <summary>
        /// Generate PDF for invoice
        /// </summary>
        Task<ApiResponseModel<byte[]>> GenerateInvoicePDFAsync(string invoiceId);
        Task<ApiResponseModel<InvoiceResponseModel>> GetInvoiceByIdAsync(string invoiceId);

        Task<ApiResponseModel<bool>> MarkInvoiceAsPaidAsync(string invoiceId, string paymentMethod);

        Task<ApiResponseModel<bool>> UpdateInvoicePaymentInfoAsync(
            string invoiceId,
            string paymentIntentId,
            decimal walletAmount,
            decimal stripeAmount);

        Task<ApiResponseModel<bool>> ProcessBrokerPaymentConfirmationAsync(
            string invoiceId,
            string paymentIntentId,
            string userId);
    }
}