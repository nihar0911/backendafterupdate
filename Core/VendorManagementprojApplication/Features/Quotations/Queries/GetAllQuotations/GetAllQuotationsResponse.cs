using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Quotations.Queries.GetAllQuotations;

public class GetAllQuotationsResponse
{
    public List<QuotationDto> Quotations { get; set; } = new();
}
