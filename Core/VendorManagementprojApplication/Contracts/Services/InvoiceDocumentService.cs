using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VendorManagementprojApplication.Contracts.Infrastructure;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Services;

public class InvoiceDocumentService : IInvoiceDocumentService
{
    public Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var items = invoice.Items?.ToList() ?? new List<InvoiceItem>();
        decimal subtotal = invoice.Subtotal;
        decimal taxAmount = invoice.TaxAmount;
        decimal totalAmount = invoice.TotalAmount;

        decimal cgstAmount = Math.Round(taxAmount / 2m, 2);
        decimal sgstAmount = taxAmount - cgstAmount;

        var distinctRates = items.Select(i => i.TaxRate).Distinct().ToList();
        string cgstLabel;
        string sgstLabel;
        if (distinctRates.Count == 1)
        {
            decimal halfRate = distinctRates[0] / 2m;
            cgstLabel = $"CGST ({halfRate:0.##}%):";
            sgstLabel = $"SGST ({halfRate:0.##}%):";
        }
        else
        {
            cgstLabel = "CGST:";
            sgstLabel = "SGST:";
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(32);
                page.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken4));

                page.Header().Element(c => ComposeHeader(c, invoice));

                page.Content().PaddingVertical(14).Element(c => ComposeContent(c, invoice, items, subtotal, cgstAmount, sgstAmount, taxAmount, totalAmount, cgstLabel, sgstLabel));

                page.Footer().Element(ComposeFooter);
            });
        });

        var pdfBytes = document.GeneratePdf();

        return Task.FromResult(pdfBytes);
    }

    private static void ComposeHeader(IContainer container, Invoice invoice)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("INVOICE").FontSize(22).Bold().FontColor(Colors.BlueGrey.Darken4);
                    col.Item().Text($"Invoice #: INV-#{invoice.InvoiceID}").FontSize(10).SemiBold().FontColor(Colors.Grey.Darken2);
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text($"Invoice Date: {invoice.InvoiceDate:dd-MM-yyyy}").FontSize(10).FontColor(Colors.Grey.Darken3);
                    col.Item().Text($"Purchase Order: PO-#{invoice.PurchaseOrderID}").FontSize(10).FontColor(Colors.Grey.Darken3);

                    if (!string.IsNullOrWhiteSpace(invoice.Status))
                    {
                        col.Item().Text($"Status: {invoice.Status}").FontSize(10).Bold().FontColor(Colors.BlueGrey.Darken2);
                    }

                    if (!string.IsNullOrWhiteSpace(invoice.PurchaseOrder?.DeliveryStatus))
                    {
                        col.Item().Text($"Delivery: {invoice.PurchaseOrder.DeliveryStatus}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    }
                });
            });

            column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    private static void ComposeContent(
        IContainer container,
        Invoice invoice,
        List<InvoiceItem> items,
        decimal subtotal,
        decimal cgstAmount,
        decimal sgstAmount,
        decimal taxAmount,
        decimal totalAmount,
        string cgstLabel,
        string sgstLabel)
    {
        container.Column(column =>
        {
            // Vendor and Buyer Details
            column.Item().Row(row =>
            {
                // Vendor Info (Issued By)
                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Column(vendorCol =>
                {
                    vendorCol.Item().Text("ISSUED BY — VENDOR").FontSize(9).Bold().FontColor(Colors.BlueGrey.Darken3);
                    vendorCol.Item().PaddingTop(4).Text(invoice.Vendor?.VendorName ?? "Not available").FontSize(10).Bold();

                    if (!string.IsNullOrWhiteSpace(invoice.Vendor?.Address))
                        vendorCol.Item().Text(invoice.Vendor.Address).FontSize(8.5f).FontColor(Colors.Grey.Darken2);

                    if (!string.IsNullOrWhiteSpace(invoice.Vendor?.GSTIN))
                        vendorCol.Item().PaddingTop(2).Text($"GSTIN: {invoice.Vendor.GSTIN}").FontSize(8.5f).SemiBold().FontColor(Colors.Grey.Darken3);

                    if (!string.IsNullOrWhiteSpace(invoice.Vendor?.Phone))
                        vendorCol.Item().Text($"Phone: {invoice.Vendor.Phone}").FontSize(8.5f).FontColor(Colors.Grey.Darken2);

                    if (!string.IsNullOrWhiteSpace(invoice.Vendor?.Email))
                        vendorCol.Item().Text($"Email: {invoice.Vendor.Email}").FontSize(8.5f).FontColor(Colors.Grey.Darken2);
                });

                row.ConstantItem(12);

                // Buyer Info (Bill To)
                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Column(buyerCol =>
                {
                    buyerCol.Item().Text("BILL TO — BUYER").FontSize(9).Bold().FontColor(Colors.BlueGrey.Darken3);
                    buyerCol.Item().PaddingTop(4).Text(invoice.Outlet?.Organization?.OrganizationName ?? "Not available").FontSize(10).Bold();

                    if (!string.IsNullOrWhiteSpace(invoice.Outlet?.Organization?.Address))
                        buyerCol.Item().Text(invoice.Outlet.Organization.Address).FontSize(8.5f).FontColor(Colors.Grey.Darken2);

                    if (!string.IsNullOrWhiteSpace(invoice.Outlet?.Organization?.Phone))
                        buyerCol.Item().Text($"Phone: {invoice.Outlet.Organization.Phone}").FontSize(8.5f).FontColor(Colors.Grey.Darken2);

                    if (!string.IsNullOrWhiteSpace(invoice.Outlet?.Organization?.Email))
                        buyerCol.Item().Text($"Email: {invoice.Outlet.Organization.Email}").FontSize(8.5f).FontColor(Colors.Grey.Darken2);

                    buyerCol.Item().PaddingTop(4).Text($"Outlet: {invoice.Outlet?.OutletName ?? "Not available"}").FontSize(8.5f).SemiBold().FontColor(Colors.Grey.Darken3);

                    if (!string.IsNullOrWhiteSpace(invoice.Outlet?.Address))
                        buyerCol.Item().Text(invoice.Outlet.Address).FontSize(8.5f).FontColor(Colors.Grey.Darken2);
                });
            });

            // Items Table
            column.Item().PaddingTop(14).Element(c => ComposeTable(c, items));

            // Tax Summary & Grand Total
            column.Item().PaddingTop(12).Row(row =>
            {
                row.RelativeItem(); // Spacer on left

                row.ConstantItem(240).Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Column(summaryCol =>
                {
                    summaryCol.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Subtotal:").FontSize(9).FontColor(Colors.Grey.Darken3);
                        r.RelativeItem().AlignRight().Text($"₹{subtotal:N2}").FontSize(9).FontColor(Colors.Grey.Darken3);
                    });

                    summaryCol.Item().PaddingTop(3).Row(r =>
                    {
                        r.RelativeItem().Text(cgstLabel).FontSize(9).FontColor(Colors.Grey.Darken3);
                        r.RelativeItem().AlignRight().Text($"₹{cgstAmount:N2}").FontSize(9).FontColor(Colors.Grey.Darken3);
                    });

                    summaryCol.Item().PaddingTop(3).Row(r =>
                    {
                        r.RelativeItem().Text(sgstLabel).FontSize(9).FontColor(Colors.Grey.Darken3);
                        r.RelativeItem().AlignRight().Text($"₹{sgstAmount:N2}").FontSize(9).FontColor(Colors.Grey.Darken3);
                    });

                    summaryCol.Item().PaddingTop(3).Row(r =>
                    {
                        r.RelativeItem().Text("Total Tax:").FontSize(9).SemiBold().FontColor(Colors.Grey.Darken3);
                        r.RelativeItem().AlignRight().Text($"₹{taxAmount:N2}").FontSize(9).SemiBold().FontColor(Colors.Grey.Darken3);
                    });

                    summaryCol.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    summaryCol.Item().PaddingTop(6).Row(r =>
                    {
                        r.RelativeItem().Text("GRAND TOTAL:").FontSize(11).Bold().FontColor(Colors.BlueGrey.Darken4);
                        r.RelativeItem().AlignRight().Text($"₹{totalAmount:N2}").FontSize(11).Bold().FontColor(Colors.BlueGrey.Darken4);
                    });
                });
            });
        });
    }

    private static void ComposeTable(IContainer container, List<InvoiceItem> items)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(26);       // #
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
                        .PaddingVertical(5)
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
                    .PaddingVertical(4)
                    .PaddingHorizontal(4)
                    .DefaultTextStyle(x => x.FontSize(8.5f).FontColor(Colors.Grey.Darken4));
            }
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text("Vendor Management System — Enterprise Procurement Platform").FontSize(8).FontColor(Colors.Grey.Darken1);

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