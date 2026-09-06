using MediatR;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Commands.UpdatePurchaseRequest;

public class UpdatePurchaseRequestCommand
    : IRequest<UpdatePurchaseRequestResponse>
{
    public int RequestID { get; set; }

    public int OutletID { get; set; }

    public int UpdatedByUserID { get; set; }
}