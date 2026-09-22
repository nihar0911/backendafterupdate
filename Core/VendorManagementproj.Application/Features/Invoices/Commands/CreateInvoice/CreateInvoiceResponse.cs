using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Invoices.Commands.CreateInvoice;

public class CreateInvoiceResponse
{
    public InvoiceDto Invoice { get; set; } = null!;
}