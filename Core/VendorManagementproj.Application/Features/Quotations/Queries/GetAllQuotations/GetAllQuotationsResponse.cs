using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Quotations.Queries.GetAllQuotations;

public class GetAllQuotationsResponse
{
    public List<QuotationDto> Quotations { get; set; } = new();
}
