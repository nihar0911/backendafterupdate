using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Invoices.Commands.RejectInvoice;

public class RejectInvoiceResponse
{
    public InvoiceDto? Invoice { get; set; }
}