using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using trucki.DatabaseContext;
using trucki.Entities;
using trucki.Interfaces.IServices;
using Microsoft.EntityFrameworkCore;

namespace trucki.Services;

public class PDFService : IPDFService
{
    private readonly TruckiDBContext _dbContext;

    public PDFService(TruckiDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<byte[]> GenerateInvoicePDFAsync(string invoiceId)
    {
        var invoice = await _dbContext.Set<Invoice>()
            .Include(i => i.Order)
                .ThenInclude(o => o.CargoOwner)
            .Include(i => i.Order.AcceptedBid)
                .ThenInclude(b => b.Truck)
                    .ThenInclude(t => t.Driver)
            .Include(i => i.Order.Items)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceId);

        if (invoice == null)
            throw new Exception("Invoice not found");
        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "logo.png");
        var logoBytes = File.ReadAllBytes(logoPath);
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);

                page.Header().Row(row =>
                {
                    row.RelativeColumn().Text($"Invoice #{invoice.InvoiceNumber}").FontSize(20).Bold();
                    // row.ConstantColumn(100).Height(50).Image(logoBytes).FitWidth();
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Text($"Created: {invoice.CreatedAt:MMMM dd, yyyy}").FontSize(12);
                    col.Item().Text($"Due Date: {invoice.DueDate:MMMM dd, yyyy}").FontSize(12);

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    col.Item().Row(row =>
                    {
                        row.RelativeColumn().Column(inner =>
                        {
                            inner.Item().Text("From:").Bold();
                            inner.Item().Text("Trucki Limited");
                            inner.Item().Text("12 Trans-Amadi Road");
                            inner.Item().Text("Port Harcourt, Nigeria");
                        });

                        row.RelativeColumn().Column(inner =>
                        {
                            inner.Item().Text("Bill To:").Bold();
                            inner.Item().Text(invoice.Order.CargoOwner.CompanyName);
                            inner.Item().Text(invoice.Order.CargoOwner.Name);
                            inner.Item().Text(invoice.Order.CargoOwner.Phone);
                            inner.Item().Text(invoice.Order.CargoOwner.EmailAddress);
                        });
                    });

                    col.Item().PaddingVertical(10);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.ConstantColumn(50);
                            columns.ConstantColumn(60);
                            columns.ConstantColumn(80);
                        });
                        static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Item").Bold();
                            header.Cell().Element(CellStyle).Text("Type").Bold();
                            header.Cell().Element(CellStyle).Text("Qty").Bold();
                            header.Cell().Element(CellStyle).Text("Weight").Bold();
                            header.Cell().Element(CellStyle).Text("Total Weight").Bold();

                        });

                        decimal totalWeight = 0;

                        foreach (var item in invoice.Order.Items)
                        {
                            var itemTotal = item.Weight * item.Quantity;
                            totalWeight += itemTotal;

                            table.Cell().Element(CellStyle).Text(item.Description);
                            table.Cell().Element(CellStyle).Text(item.Type);
                            table.Cell().Element(CellStyle).Text(item.Quantity.ToString());
                            table.Cell().Element(CellStyle).Text($"{item.Weight} kg");
                            table.Cell().Element(CellStyle).Text($"{itemTotal} kg");
                        }

                        table.Cell().ColumnSpan(4).AlignRight().Text("Total Weight:").Bold();
                        table.Cell().Text($"{totalWeight} kg").Bold();
                    });

                    col.Item().PaddingVertical(10);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.ConstantColumn(150);
                        });

                        table.Cell().Text($"Transport from {invoice.Order.PickupLocation} to {invoice.Order.DeliveryLocation}");
                        table.Cell().AlignRight().Text(invoice.SubTotal.ToString("C", new CultureInfo("en-NG")));

                        table.Cell().Text("System Fee (10%)");
                        table.Cell().AlignRight().Text(invoice.SystemFee.ToString("C", new CultureInfo("en-NG")));

                        table.Cell().Text("VAT (10%)");
                        table.Cell().AlignRight().Text(invoice.Tax.ToString("C", new CultureInfo("en-NG")));

                        table.Cell().Element(container => container.PaddingTop(5).BorderTop(1).BorderColor(Colors.Grey.Lighten2)).Text("Total").Bold();
                        table.Cell().Element(container => container.PaddingTop(5).BorderTop(1).BorderColor(Colors.Grey.Lighten2)).AlignRight().Text(invoice.TotalAmount.ToString("C", new CultureInfo("en-NG"))).Bold();
                    });

                    col.Item().PaddingVertical(15);
                    col.Item().Text("Payment Terms:").Bold();
                    col.Item().Text("Payment is due within 7 days of receiving this invoice. Bank: Guaranty Trust Bank, Account: Trucki Limited, Account Number: 0123456789, Swift Code: GTBINGLA").FontSize(10);

                    col.Item().PaddingTop(10).Text("Notes:").Bold();
                    col.Item().Text("Please include the invoice number in your payment reference. For any inquiries, email account@trucki.co").FontSize(10);
                });
            });
        });

        return document.GeneratePdf();
    }
}
