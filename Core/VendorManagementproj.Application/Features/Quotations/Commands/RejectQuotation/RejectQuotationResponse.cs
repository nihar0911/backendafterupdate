using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Quotations.Commands.RejectQuotation;

public class RejectQuotationResponse
{
    public QuotationDto Quotation { get; set; } = null!;
}
