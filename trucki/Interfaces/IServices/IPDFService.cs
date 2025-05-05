using System.Threading.Tasks;

namespace trucki.Interfaces.IServices;

public interface IPDFService
{
    Task<byte[]> GenerateInvoicePDFAsync(string invoiceId);
}