using MediatR;

namespace VendorManagementproj.Application.Features.PurchaseRequests.Commands.UpdatePurchaseRequest;

public class UpdatePurchaseRequestCommand
    : IRequest<UpdatePurchaseRequestResponse>
{
    public int RequestID { get; set; }

    public int OutletID { get; set; }

    public int UpdatedByUserID { get; set; }
}