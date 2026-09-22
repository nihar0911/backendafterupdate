using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Quotations.Commands.RespondToQuotation;

public class RespondToQuotationResponse
{
    public QuotationDto Quotation { get; set; } = null!;
}