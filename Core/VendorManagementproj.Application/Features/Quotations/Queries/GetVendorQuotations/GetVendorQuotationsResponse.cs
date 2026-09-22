using System.Collections.Generic;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Quotations.Queries.GetVendorQuotations;

public class GetVendorQuotationsResponse
{
    public List<QuotationDto> Quotations { get; set; } = new();
}
