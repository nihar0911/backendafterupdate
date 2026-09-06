using MediatR;

namespace VendorManagementprojApplication.Features.Quotations.Commands.RespondToQuotation;

public class RespondToQuotationCommand
    : IRequest<RespondToQuotationResponse>
{
    public int QuotationID { get; set; }

    public string Status { get; set; } = string.Empty;
}