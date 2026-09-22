using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Quotations.Commands.RespondToQuotation;

public class RespondToQuotationResponse
{
    public QuotationDto Quotation { get; set; } = null!;
}