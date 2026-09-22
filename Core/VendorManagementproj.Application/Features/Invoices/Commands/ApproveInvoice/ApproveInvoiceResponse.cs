using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Invoices.Commands.ApproveInvoice;

public class ApproveInvoiceResponse
{
    public InvoiceDto? Invoice { get; set; }
}