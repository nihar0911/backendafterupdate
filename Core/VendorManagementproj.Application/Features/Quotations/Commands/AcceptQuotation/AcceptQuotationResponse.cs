using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Quotations.Commands.AcceptQuotation;

public class AcceptQuotationResponse
{
    public QuotationDto Quotation { get; set; } = null!;
}
