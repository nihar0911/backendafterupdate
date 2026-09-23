using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VendorManagementproj.Application.Contracts.Infrastructure;
using VendorManagementproj.Domain.Entities;

namespace VendorManagementproj.Application.Services;

public class PaymentDocumentService : IPaymentDocumentService
{
    public Task<byte[]> GeneratePaymentReceiptPdfAsync(Payment payment)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var invoice = payment.Invoice;
        var items = invoice?.Items?.ToList() ?? new List<InvoiceItem>();
        decimal subtotal = invoice?.Subtotal ?? payment.Amount;
        decimal taxAmount = invoice?.TaxAmount ?? 0m;
        decimal totalAmount = invoice?.TotalAmount ?? payment.Amount;

        decimal cgstAmount = Math.Round(taxAmount / 2m, 2);
        decimal sgstAmount = taxAmount - cgstAmount;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(32);
                page.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken4));

                page.Header().Element(c => ComposeHeader(c, payment, invoice));

                page.Content().PaddingVertical(14).Element(c => ComposeContent(c, payment, invoice, items, subtotal, cgstAmount, sgstAmount, taxAmount, totalAmount));

                page.Footer().Element(ComposeFooter);
            });
        });

        var pdfBytes = document.GeneratePdf();
        return Task.FromResult(pdfBytes);
    }

    private static void ComposeHeader(IContainer container, Payment payment, Invoice? invoice)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("PAYMENT RECEIPT").FontSize(22).Bold().FontColor(Colors.BlueGrey.Darken4);
                    col.Item().Text($"Receipt #: PAY-#{payment.PaymentID}").FontSize(10).SemiBold().FontColor(Colors.Teal.Darken3);
                    col.Item().PaddingTop(3).Row(badgeRow =>
                    {
                        badgeRow.AutoItem().Background(Colors.Green.Lighten5).Border(1).BorderColor(Colors.Green.Darken1).PaddingVertical(2).PaddingHorizontal(6).Text("STATUS: PAID / SETTLED").FontSize(8).Bold().FontColor(Colors.Green.Darken3);
                    });
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text($"Payment Date: {payment.PaymentDate:dd-MM-yyyy}").FontSize(10).FontColor(Colors.Grey.Darken3);
                    if (invoice != null)
                    {
                        col.Item().Text($"Settled Invoice: INV-#{invoice.InvoiceID}").FontSize(10).SemiBold().FontColor(Colors.Grey.Darken3);
                        col.Item().Text($"Purchase Order: PO-#{invoice.PurchaseOrderID}").FontSize(10).FontColor(Colors.Grey.Darken3);
                    }
                    if (!string.IsNullOrWhiteSpace(payment.PaymentMethod))
                    {
                        col.Item().Text($"Method: {payment.PaymentMethod}").FontSize(9).FontColor(Colors.Grey.Darken2);
                    }
                });
            });

            column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    private static void ComposeContent(
        IContainer container,
        Payment payment,
        Invoice? invoice,
        List<InvoiceItem> items,
        decimal subtotal,
        decimal cgstAmount,
        decimal sgstAmount,
        decimal taxAmount,
        decimal totalAmount)
    {
        container.Column(column =>
        {
            // Payer and Payee Section
            column.Item().Row(row =>
            {
                // Payer Details (Paid By)
                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Column(payerCol =>
                {
                    payerCol.Item().Text("PAID BY — BUYER").FontSize(9).Bold().FontColor(Colors.BlueGrey.Darken3);
                    
                    var orgName = invoice?.Outlet?.Organization?.OrganizationName 
                        ?? invoice?.PurchaseOrder?.Outlet?.Organization?.OrganizationName 
                        ?? "Organization";
                    payerCol.Item().PaddingTop(4).Text(orgName).FontSize(10).Bold();

                    var orgAddress = invoice?.Outlet?.Organization?.Address ?? invoice?.PurchaseOrder?.Outlet?.Organization?.Address;
                    if (!string.IsNullOrWhiteSpace(orgAddress))
                        payerCol.Item().Text(orgAddress).FontSize(8.5f).FontColor(Colors.Grey.Darken2);

                    var orgPhone = invoice?.Outlet?.Organization?.Phone ?? invoice?.PurchaseOrder?.Outlet?.Organization?.Phone;
                    if (!string.IsNullOrWhiteSpace(orgPhone))
                        payerCol.Item().Text($"Phone: {orgPhone}").FontSize(8.5f).FontColor(Colors.Grey.Darken2);

                    var orgEmail = invoice?.Outlet?.Organization?.Email ?? invoice?.PurchaseOrder?.Outlet?.Organization?.Email;
                    if (!string.IsNullOrWhiteSpace(orgEmail))
                        payerCol.Item().Text($"Email: {orgEmail}").FontSize(8.5f).FontColor(Colors.Grey.Darken2);

                    var outletName = invoice?.Outlet?.OutletName ?? invoice?.PurchaseOrder?.Outlet?.OutletName;
                    if (!string.IsNullOrWhiteSpace(outletName))
                    {
                        payerCol.Item().PaddingTop(4).Text($"Outlet: {outletName}").FontSize(8.5f).SemiBold().FontColor(Colors.Grey.Darken3);
                        var outletAddress = invoice?.Outlet?.Address ?? invoice?.PurchaseOrder?.Outlet?.Address;
                        if (!string.IsNullOrWhiteSpace(outletAddress))
                            payerCol.Item().Text(outletAddress).FontSize(8.5f).FontColor(Colors.Grey.Darken2);
                    }
                });

                row.ConstantItem(12);

                // Payee Details (Paid To)
                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Column(payeeCol =>
                {
                    payeeCol.Item().Text("PAID TO — BENEFICIARY / VENDOR").FontSize(9).Bold().FontColor(Colors.BlueGrey.Darken3);
                    
                    var vendorName = invoice?.Vendor?.VendorName ?? invoice?.PurchaseOrder?.Vendor?.VendorName ?? "Vendor";
                    payeeCol.Item().PaddingTop(4).Text(vendorName).FontSize(10).Bold();

                    var vendorAddress = invoice?.Vendor?.Address ?? invoice?.PurchaseOrder?.Vendor?.Address;
                    if (!string.IsNullOrWhiteSpace(vendorAddress))
                        payeeCol.Item().Text(vendorAddress).FontSize(8.5f).FontColor(Colors.Grey.Darken2);

                    var gstin = invoice?.Vendor?.GSTIN ?? invoice?.PurchaseOrder?.Vendor?.GSTIN;
                    if (!string.IsNullOrWhiteSpace(gstin))
                        payeeCol.Item().PaddingTop(2).Text($"GSTIN: {gstin}").FontSize(8.5f).SemiBold().FontColor(Colors.Grey.Darken3);

                    var vendorPhone = invoice?.Vendor?.Phone ?? invoice?.PurchaseOrder?.Vendor?.Phone;
                    if (!string.IsNullOrWhiteSpace(vendorPhone))
                        payeeCol.Item().Text($"Phone: {vendorPhone}").FontSize(8.5f).FontColor(Colors.Grey.Darken2);

                    var vendorEmail = invoice?.Vendor?.Email ?? invoice?.PurchaseOrder?.Vendor?.Email;
                    if (!string.IsNullOrWhiteSpace(vendorEmail))
                        payeeCol.Item().Text($"Email: {vendorEmail}").FontSize(8.5f).FontColor(Colors.Grey.Darken2);
                });
            });

            // Prominent Settlement Card
            column.Item().PaddingTop(12).Border(1).BorderColor(Colors.Teal.Lighten2).Background(Colors.Teal.Lighten5).Padding(12).Column(settleCol =>
            {
                settleCol.Item().Row(settleRow =>
                {
                    settleRow.RelativeItem(2).Column(amountCol =>
                    {
                        amountCol.Item().Text("AMOUNT SETTLED").FontSize(8.5f).Bold().FontColor(Colors.Teal.Darken3);
                        amountCol.Item().Text($"₹{payment.Amount:N2}").FontSize(18).Bold().FontColor(Colors.Teal.Darken4);
                    });

                    settleRow.RelativeItem(3).Column(detailsCol =>
                    {
                        detailsCol.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Payment Method:").FontSize(8.5f).SemiBold().FontColor(Colors.Grey.Darken3);
                            r.RelativeItem(2).Text(payment.PaymentMethod).FontSize(8.5f).FontColor(Colors.Grey.Darken4);
                        });

                        detailsCol.Item().PaddingTop(2).Row(r =>
                        {
                            r.RelativeItem().Text("Transaction / Ref:").FontSize(8.5f).SemiBold().FontColor(Colors.Grey.Darken3);
                            r.RelativeItem(2).Text(string.IsNullOrWhiteSpace(payment.TransactionReference) ? "Direct Settlement / Reference N/A" : payment.TransactionReference).FontSize(8.5f).FontColor(Colors.Grey.Darken4);
                        });

                        detailsCol.Item().PaddingTop(2).Row(r =>
                        {
                            r.RelativeItem().Text("Settlement Date:").FontSize(8.5f).SemiBold().FontColor(Colors.Grey.Darken3);
                            r.RelativeItem(2).Text($"{payment.PaymentDate:dd MMM yyyy, hh:mm tt}").FontSize(8.5f).FontColor(Colors.Grey.Darken4);
                        });
                    });
                });
            });

            // Settled Items Breakdown Table (if available)
            if (items.Count > 0)
            {
                column.Item().PaddingTop(14).Text("SETTLED INVOICE LINE ITEMS").FontSize(9.5f).Bold().FontColor(Colors.BlueGrey.Darken4);
                column.Item().PaddingTop(4).Element(c => ComposeItemsTable(c, items));
            }

            // Summary Totals Card
            column.Item().PaddingTop(12).Row(row =>
            {
                row.RelativeItem(); // Spacer

                row.ConstantItem(240).Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Column(summaryCol =>
                {
                    if (taxAmount > 0 || subtotal != payment.Amount)
                    {
                        summaryCol.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Invoice Subtotal:").FontSize(9).FontColor(Colors.Grey.Darken3);
                            r.RelativeItem().AlignRight().Text($"₹{subtotal:N2}").FontSize(9).FontColor(Colors.Grey.Darken3);
                        });

                        summaryCol.Item().PaddingTop(3).Row(r =>
                        {
                            r.RelativeItem().Text("CGST:").FontSize(9).FontColor(Colors.Grey.Darken3);
                            r.RelativeItem().AlignRight().Text($"₹{cgstAmount:N2}").FontSize(9).FontColor(Colors.Grey.Darken3);
                        });

                        summaryCol.Item().PaddingTop(3).Row(r =>
                        {
                            r.RelativeItem().Text("SGST:").FontSize(9).FontColor(Colors.Grey.Darken3);
                            r.RelativeItem().AlignRight().Text($"₹{sgstAmount:N2}").FontSize(9).FontColor(Colors.Grey.Darken3);
                        });

                        summaryCol.Item().PaddingTop(3).Row(r =>
                        {
                            r.RelativeItem().Text("Total Tax:").FontSize(9).SemiBold().FontColor(Colors.Grey.Darken3);
                            r.RelativeItem().AlignRight().Text($"₹{taxAmount:N2}").FontSize(9).SemiBold().FontColor(Colors.Grey.Darken3);
                        });

                        summaryCol.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    }

                    summaryCol.Item().PaddingTop(taxAmount > 0 ? 6 : 0).Row(r =>
                    {
                        r.RelativeItem().Text("TOTAL PAID:").FontSize(11).Bold().FontColor(Colors.Teal.Darken4);
                        r.RelativeItem().AlignRight().Text($"₹{payment.Amount:N2}").FontSize(11).Bold().FontColor(Colors.Teal.Darken4);
                    });
                });
            });
        });
    }

    private static void ComposeItemsTable(IContainer container, List<InvoiceItem> items)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(24);       // #
                columns.RelativeColumn(3);        // Product
                columns.RelativeColumn(1);        // Qty
                columns.RelativeColumn(1);        // Unit
                columns.RelativeColumn(1.3f);     // Unit Price
                columns.RelativeColumn(1);        // Tax %
                columns.RelativeColumn(1.3f);     // Tax Amount
                columns.RelativeColumn(1.4f);     // Amount
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderStyle).AlignCenter().Text("#");
                header.Cell().Element(HeaderStyle).Text("Product");
                header.Cell().Element(HeaderStyle).AlignRight().Text("Qty");
                header.Cell().Element(HeaderStyle).AlignCenter().Text("Unit");
                header.Cell().Element(HeaderStyle).AlignRight().Text("Unit Price");
                header.Cell().Element(HeaderStyle).AlignRight().Text("Tax %");
                header.Cell().Element(HeaderStyle).AlignRight().Text("Tax Amount");
                header.Cell().Element(HeaderStyle).AlignRight().Text("Amount");

                static IContainer HeaderStyle(IContainer cellContainer)
                {
                    return cellContainer
                        .Background(Colors.Grey.Lighten3)
                        .Border(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .PaddingVertical(4)
                        .PaddingHorizontal(4)
                        .DefaultTextStyle(x => x.FontSize(8.5f).Bold().FontColor(Colors.BlueGrey.Darken4));
                }
            });

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var index = i + 1;
                var productName = !string.IsNullOrWhiteSpace(item.Product?.ProductName)
                    ? item.Product.ProductName
                    : $"Product #{item.ProductID}";
                var unit = !string.IsNullOrWhiteSpace(item.Product?.Unit)
                    ? item.Product.Unit
                    : "-";

                table.Cell().Element(CellStyle).AlignCenter().Text(index.ToString());
                table.Cell().Element(CellStyle).Text(productName);
                table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString("G29"));
                table.Cell().Element(CellStyle).AlignCenter().Text(unit);
                table.Cell().Element(CellStyle).AlignRight().Text($"₹{item.UnitPrice:N2}");
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.TaxRate:0.##}%");
                table.Cell().Element(CellStyle).AlignRight().Text($"₹{item.TaxAmount:N2}");
                table.Cell().Element(CellStyle).AlignRight().Text($"₹{item.TotalAmount:N2}");
            }

            static IContainer CellStyle(IContainer cellContainer)
            {
                return cellContainer
                    .BorderBottom(1)
                    .BorderColor(Colors.Grey.Lighten3)
                    .PaddingVertical(3)
                    .PaddingHorizontal(4)
                    .DefaultTextStyle(x => x.FontSize(8.5f).FontColor(Colors.Grey.Darken4));
            }
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text("Vendor Management System — Official Payment Receipt Voucher").FontSize(8).FontColor(Colors.Grey.Darken1);

            row.RelativeItem().AlignRight().Text(text =>
            {
                text.Span("Page ").FontSize(8).FontColor(Colors.Grey.Darken1);
                text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                text.Span(" of ").FontSize(8).FontColor(Colors.Grey.Darken1);
                text.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        });
    }
}
