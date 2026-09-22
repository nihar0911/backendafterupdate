using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Quotations.Commands.CreateQuotation;

public class CreateQuotationResponse
{
    public QuotationDto Quotation { get; set; } = null!;
}
