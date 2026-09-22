using System.Collections.Generic;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Invoices.Queries.GetAllInvoices;

public class GetAllInvoicesResponse
{
    public List<InvoiceDto> Invoices { get; set; } = new();
}