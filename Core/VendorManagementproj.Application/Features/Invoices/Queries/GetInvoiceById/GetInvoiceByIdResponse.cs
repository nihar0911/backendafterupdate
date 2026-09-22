using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Invoices.Queries.GetInvoiceById;

public class GetInvoiceByIdResponse
{
    public InvoiceDto Invoice { get; set; } = null!;
}