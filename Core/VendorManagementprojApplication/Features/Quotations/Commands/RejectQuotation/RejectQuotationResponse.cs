using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Quotations.Commands.RejectQuotation;

public class RejectQuotationResponse
{
    public QuotationDto Quotation { get; set; } = null!;
}
