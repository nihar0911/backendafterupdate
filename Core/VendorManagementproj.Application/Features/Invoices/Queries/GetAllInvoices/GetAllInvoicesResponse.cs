using System.Collections.Generic;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Invoices.Queries.GetAllInvoices;

public class GetAllInvoicesResponse
{
    public List<InvoiceDto> Invoices { get; set; } = new();
}