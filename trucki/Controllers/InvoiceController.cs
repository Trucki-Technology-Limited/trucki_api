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

        [HttpGet("GetCargoOwnerInvoices/{cargoOwnerId}")]
        public async Task<IActionResult> GetCargoOwnerInvoices(
            string cargoOwnerId,
            [FromQuery] GetInvoicesQueryDto query)
        {
            var result = await _invoiceService.GetCargoOwnerInvoicesAsync(cargoOwnerId, query);
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
    }
}