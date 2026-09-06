using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Invoices.Commands.RejectInvoice;

public class RejectInvoiceResponse
{
    public InvoiceDto? Invoice { get; set; }
}