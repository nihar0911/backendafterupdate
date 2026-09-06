using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Quotations.Commands.CreateQuotation;

public class CreateQuotationResponse
{
    public QuotationDto Quotation { get; set; } = null!;
}
