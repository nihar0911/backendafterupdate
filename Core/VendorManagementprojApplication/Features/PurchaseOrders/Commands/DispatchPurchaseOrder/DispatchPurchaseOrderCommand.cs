using MediatR;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Commands.DispatchPurchaseOrder;

public class DispatchPurchaseOrderCommand : IRequest<DispatchPurchaseOrderResponse>
{
    public int PurchaseOrderID { get; set; }
    public int VendorID { get; set; }
}