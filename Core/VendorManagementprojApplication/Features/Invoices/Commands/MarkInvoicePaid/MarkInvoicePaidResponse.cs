using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Invoices.Commands.MarkInvoicePaid;

public class MarkInvoicePaidResponse
{
    public InvoiceDto Invoice { get; set; } = null!;
}