using MediatR;

namespace VendorManagementproj.Application.Features.PurchaseOrders.Commands.RejectPurchaseOrder;

public class RejectPurchaseOrderCommand : IRequest<RejectPurchaseOrderResponse>
{
    public int PurchaseOrderID { get; set; }
}
