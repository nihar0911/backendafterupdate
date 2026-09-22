using MediatR;

namespace VendorManagementproj.Application.Features.Quotations.Commands.AcceptQuotation;

public class AcceptQuotationCommand : IRequest<AcceptQuotationResponse>
{
    public int QuotationID { get; set; }
}
