using MediatR;

namespace VendorManagementprojApplication.Features.Quotations.Commands.RejectQuotation;

public class RejectQuotationCommand : IRequest<RejectQuotationResponse>
{
    public int QuotationID { get; set; }
}
