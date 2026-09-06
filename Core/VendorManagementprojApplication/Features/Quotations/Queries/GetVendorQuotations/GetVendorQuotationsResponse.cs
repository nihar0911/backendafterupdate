using System.Collections.Generic;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Quotations.Queries.GetVendorQuotations;

public class GetVendorQuotationsResponse
{
    public List<QuotationDto> Quotations { get; set; } = new();
}
