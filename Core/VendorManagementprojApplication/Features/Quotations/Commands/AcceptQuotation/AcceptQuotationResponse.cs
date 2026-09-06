using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Quotations.Commands.AcceptQuotation;

public class AcceptQuotationResponse
{
    public QuotationDto Quotation { get; set; } = null!;
}
