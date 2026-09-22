using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Invoices.Commands.ApproveInvoice;

public class ApproveInvoiceResponse
{
    public InvoiceDto? Invoice { get; set; }
}