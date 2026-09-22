using MediatR;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Commands.RejectPurchaseOrder;

public class RejectPurchaseOrderCommand : IRequest<RejectPurchaseOrderResponse>
{
    public int PurchaseOrderID { get; set; }
}
