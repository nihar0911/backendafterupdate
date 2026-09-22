using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Invoices.Queries.GetInvoiceById;

public class GetInvoiceByIdResponse
{
    public InvoiceDto Invoice { get; set; } = null!;
}