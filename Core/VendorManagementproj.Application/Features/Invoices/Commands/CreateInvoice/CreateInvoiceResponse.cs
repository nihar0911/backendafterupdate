using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Invoices.Commands.CreateInvoice;

public class CreateInvoiceResponse
{
    public InvoiceDto Invoice { get; set; } = null!;
}