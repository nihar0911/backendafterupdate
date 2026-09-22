using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Invoices.Commands.MarkInvoicePaid;

public class MarkInvoicePaidResponse
{
    public InvoiceDto Invoice { get; set; } = null!;
}