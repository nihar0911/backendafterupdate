using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VendorManagementprojApplication.Contracts.Infrastructure;

namespace VendorManagementprojApplication.Services;

public class InvoiceDocumentService : IInvoiceDocumentService
{
    public Task<byte[]> GenerateInvoicePdfAsync(
        int invoiceID,
        int purchaseOrderID,
        decimal subtotal,
        decimal taxAmount,
        decimal totalAmount)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);

                page.Header()
                    .Text($"INVOICE #{invoiceID}")
                    .FontSize(24)
                    .Bold();

                page.Content()
                    .PaddingVertical(20)
                    .Column(column =>
                    {
                        column.Item()
                            .Text($"Purchase Order: {purchaseOrderID}")
                            .FontSize(12);

                        column.Item()
                            .Text($"Invoice Date: {DateTime.Now:dd-MM-yyyy}")
                            .FontSize(12);

                        column.Item()
                            .PaddingTop(20)
                            .Text($"Subtotal: ₹{subtotal:F2}")
                            .FontSize(12);

                        column.Item()
                            .Text($"Tax: ₹{taxAmount:F2}")
                            .FontSize(12);

                        column.Item()
                            .PaddingTop(10)
                            .Text($"Total Amount: ₹{totalAmount:F2}")
                            .FontSize(16)
                            .Bold();
                    });

                page.Footer()
                    .AlignCenter()
                    .Text("Vendor Management System");
            });
        });

        var pdfBytes = document.GeneratePdf();

        return Task.FromResult(pdfBytes);
    }
}