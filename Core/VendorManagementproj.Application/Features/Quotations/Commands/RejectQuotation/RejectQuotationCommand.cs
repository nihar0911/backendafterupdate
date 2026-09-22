using MediatR;

namespace VendorManagementproj.Application.Features.Quotations.Commands.RejectQuotation;

public class RejectQuotationCommand : IRequest<RejectQuotationResponse>
{
    public int QuotationID { get; set; }
}
