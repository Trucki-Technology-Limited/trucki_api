using Microsoft.AspNetCore.Mvc;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpPost("GenerateInvoice/{orderId}")]
        public async Task<IActionResult> GenerateInvoice(string orderId)
        {
            var result = await _invoiceService.GenerateInvoiceAsync(orderId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("GetCargoOwnerInvoices")]
        public async Task<IActionResult> GetCargoOwnerInvoices(
            [FromQuery] GetInvoicesQueryDto query)
        {
            var result = await _invoiceService.GetCargoOwnerInvoicesAsync(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("SubmitPaymentProof")]
        public async Task<IActionResult> SubmitPaymentProof([FromBody] SubmitPaymentProofDto submitPaymentDto)
        {
            var result = await _invoiceService.SubmitPaymentProofAsync(submitPaymentDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("ApprovePayment")]
        public async Task<IActionResult> ApprovePayment([FromBody] ApprovePaymentDto approvePaymentDto)
        {
            var result = await _invoiceService.ApprovePaymentAsync(approvePaymentDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("GetActiveBankAccount")]
        public async Task<IActionResult> GetActiveBankAccount()
        {
            var result = await _invoiceService.GetActiveBankAccountAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("DownloadInvoicePDF/{invoiceId}")]
        public async Task<IActionResult> DownloadInvoicePDF(string invoiceId)
        {
            var result = await _invoiceService.GenerateInvoicePDFAsync(invoiceId);

            if (!result.IsSuccessful)
            {
                return StatusCode(result.StatusCode, result);
            }

            // Return the PDF as a file download
            return File(
                result.Data,
                "application/pdf",
                $"Invoice-{invoiceId}.pdf");
        }

        [HttpGet("GetInvoicePDF/{invoiceId}")]
        public async Task<IActionResult> GetInvoicePDF(string invoiceId)
        {
            var result = await _invoiceService.GenerateInvoicePDFAsync(invoiceId);
            return StatusCode(result.StatusCode, result);
        }
    }
}