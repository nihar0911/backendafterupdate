using MediatR;

namespace VendorManagementproj.Application.Features.PurchaseOrders.Commands.DispatchPurchaseOrder;

public class DispatchPurchaseOrderCommand : IRequest<DispatchPurchaseOrderResponse>
{
    public int PurchaseOrderID { get; set; }
    public int VendorID { get; set; }
}