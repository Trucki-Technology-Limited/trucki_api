using AutoMapper;
using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Services;
public class InvoiceService : IInvoiceService
{
    private readonly TruckiDBContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IPDFService _pdfService;

    public InvoiceService(TruckiDBContext dbContext, IMapper mapper, IPDFService pdfService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _pdfService = pdfService;
    }

    public async Task<ApiResponseModel<InvoiceResponseModel>> GenerateInvoiceAsync(string orderId)
    {
        try
        {
            var order = await _dbContext.Set<CargoOrders>()
                .Include(o => o.AcceptedBid)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return ApiResponseModel<InvoiceResponseModel>.Fail("Order not found", 404);
            }

            if (order.Status != CargoOrderStatus.Delivered)
            {
                return ApiResponseModel<InvoiceResponseModel>.Fail("Order must be delivered before generating invoice", 400);
            }

            // Calculate amounts
            var subTotal = order.AcceptedBid.Amount;
            var systemFee = subTotal * 0.10m; // 20% system fee
            var tax = (subTotal + systemFee) * 0.10m; // 10% tax
            var totalAmount = subTotal + systemFee + tax;

            var invoice = new Invoice
            {
                OrderId = order.Id,
                InvoiceNumber = GenerateInvoiceNumber(),
                SubTotal = subTotal,
                SystemFee = systemFee,
                Tax = tax,
                TotalAmount = totalAmount,
                DueDate = DateTime.UtcNow.AddDays(7), // 7 days payment term
                Status = InvoiceStatus.Pending
            };

            _dbContext.Set<Invoice>().Add(invoice);
            await _dbContext.SaveChangesAsync();

            var response = _mapper.Map<InvoiceResponseModel>(invoice);
            return ApiResponseModel<InvoiceResponseModel>.Success("Invoice generated successfully", response, 200);
        }
        catch (Exception ex)
        {
            return ApiResponseModel<InvoiceResponseModel>.Fail($"Error: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponseModel<PagedResponse<InvoiceResponseModel>>> GetCargoOwnerInvoicesAsync(
      GetInvoicesQueryDto query)
    {
        try
        {
            var invoicesQuery = _dbContext.Set<Invoice>()
                .Include(i => i.Order)
                    .ThenInclude(o => o.Items) // Include the order items
                .Where(i => i.Order.CargoOwnerId == query.cargoOwnerId);

            // Apply filters
            if (query.StartDate.HasValue)
                invoicesQuery = invoicesQuery.Where(i => i.CreatedAt >= query.StartDate);

            if (query.EndDate.HasValue)
                invoicesQuery = invoicesQuery.Where(i => i.CreatedAt <= query.EndDate);

            if (query.Status.HasValue)
                invoicesQuery = invoicesQuery.Where(i => i.Status == query.Status);

            // Apply sorting
            invoicesQuery = query.SortBy?.ToLower() switch
            {
                "duedate" => query.SortDescending
                    ? invoicesQuery.OrderByDescending(i => i.DueDate)
                    : invoicesQuery.OrderBy(i => i.DueDate),
                "amount" => query.SortDescending
                    ? invoicesQuery.OrderByDescending(i => i.TotalAmount)
                    : invoicesQuery.OrderBy(i => i.TotalAmount),
                "status" => query.SortDescending
                    ? invoicesQuery.OrderByDescending(i => i.Status)
                    : invoicesQuery.OrderBy(i => i.Status),
                _ => invoicesQuery.OrderByDescending(i => i.CreatedAt)
            };

            // Apply pagination
            var totalCount = await invoicesQuery.CountAsync();
            var invoices = await invoicesQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var invoiceResponses = new List<InvoiceResponseModel>();

            // Process each invoice and calculate order summaries
            foreach (var invoice in invoices)
            {
                var invoiceResponse = _mapper.Map<InvoiceResponseModel>(invoice);

                // Create an enhanced order summary including locations
                if (invoice.Order != null)
                {
                    var orderSummary = new InvoiceCargoOrderSummaryModel
                    {
                        Id = invoice.Order.Id,
                        PickupLocation = invoice.Order.PickupLocation,
                        DeliveryLocation = invoice.Order.DeliveryLocation,
                        DeliveryDateTime = invoice.Order.DeliveryDateTime,
                        TotalItems = invoice.Order.Items?.Count ?? 0,
                        TotalWeight = invoice.Order.Items?.Sum(i => i.Weight * i.Quantity) ?? 0
                    };

                    // Set the Order property to the detailed order summary
                    invoiceResponse.Order = orderSummary;
                }

                invoiceResponses.Add(invoiceResponse);
            }

            var pagedResponse = new PagedResponse<InvoiceResponseModel>
            {
                Data = invoiceResponses,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
            };

            return ApiResponseModel<PagedResponse<InvoiceResponseModel>>.Success(
                "Invoices retrieved successfully",
                pagedResponse,
                200);
        }
        catch (Exception ex)
        {
            return ApiResponseModel<PagedResponse<InvoiceResponseModel>>.Fail($"Error: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponseModel<bool>> SubmitPaymentProofAsync(SubmitPaymentProofDto submitPaymentDto)
    {
        try
        {
            var invoice = await _dbContext.Set<Invoice>()
                .FirstOrDefaultAsync(i => i.Id == submitPaymentDto.InvoiceId);

            if (invoice == null)
            {
                return ApiResponseModel<bool>.Fail("Invoice not found", 404);
            }

            if (invoice.Status != InvoiceStatus.Pending && invoice.Status != InvoiceStatus.Overdue)
            {
                return ApiResponseModel<bool>.Fail("Invoice is not in a valid state for payment submission", 400);
            }

            invoice.PaymentProofUrl = submitPaymentDto.PaymentProofUrl;
            invoice.PaymentNotes = submitPaymentDto.Notes;
            invoice.PaymentSubmittedAt = DateTime.UtcNow;
            invoice.Status = InvoiceStatus.PaymentSubmitted;

            await _dbContext.SaveChangesAsync();

            return ApiResponseModel<bool>.Success("Payment proof submitted successfully", true, 200);
        }
        catch (Exception ex)
        {
            return ApiResponseModel<bool>.Fail($"Error: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponseModel<bool>> ApprovePaymentAsync(ApprovePaymentDto approvePaymentDto)
    {
        try
        {
            var invoice = await _dbContext.Set<Invoice>()
                .FirstOrDefaultAsync(i => i.Id == approvePaymentDto.InvoiceId);

            if (invoice == null)
            {
                return ApiResponseModel<bool>.Fail("Invoice not found", 404);
            }

            if (invoice.Status != InvoiceStatus.PaymentSubmitted)
            {
                return ApiResponseModel<bool>.Fail("Invoice payment has not been submitted", 400);
            }

            invoice.Status = InvoiceStatus.Paid;
            invoice.PaymentApprovedBy = approvePaymentDto.AdminId;
            invoice.PaymentApprovedAt = DateTime.UtcNow;
            invoice.PaymentNotes = approvePaymentDto.Notes;

            await _dbContext.SaveChangesAsync();

            return ApiResponseModel<bool>.Success("Payment approved successfully", true, 200);
        }
        catch (Exception ex)
        {
            return ApiResponseModel<bool>.Fail($"Error: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponseModel<BankAccountResponseModel>> GetActiveBankAccountAsync()
    {
        try
        {
            var bankAccount = await _dbContext.Set<PaymentAccount>()
                .FirstOrDefaultAsync(b => b.IsActive);

            if (bankAccount == null)
            {
                return ApiResponseModel<BankAccountResponseModel>.Fail("No active bank account found", 404);
            }

            var response = _mapper.Map<BankAccountResponseModel>(bankAccount);
            return ApiResponseModel<BankAccountResponseModel>.Success(
                "Bank account details retrieved successfully",
                response,
                200);
        }
        catch (Exception ex)
        {
            return ApiResponseModel<BankAccountResponseModel>.Fail($"Error: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponseModel<byte[]>> GenerateInvoicePDFAsync(string invoiceId)
    {
        try
        {
            // Check if invoice exists
            var invoiceExists = await _dbContext.Set<Invoice>().AnyAsync(i => i.InvoiceNumber == invoiceId);

            if (!invoiceExists)
            {
                return ApiResponseModel<byte[]>.Fail("Invoice not found", 404);
            }

            // Generate the PDF using the PDFService
            var pdfBytes = await _pdfService.GenerateInvoicePDFAsync(invoiceId);
            await File.WriteAllBytesAsync("invoice-debug.pdf", pdfBytes);

            return ApiResponseModel<byte[]>.Success(
                "Invoice PDF generated successfully",
                pdfBytes,
                200);
        }
        catch (Exception ex)
        {
            return ApiResponseModel<byte[]>.Fail($"Error generating PDF: {ex.Message}", 500);
        }
    }

    private string GenerateInvoiceNumber()
    {
        // Format: INV-YYYYMMDD-XXXX
        var dateString = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random();
        var randomPart = random.Next(1000, 9999).ToString();
        return $"INV-{dateString}-{randomPart}";
    }
}