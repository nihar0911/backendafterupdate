using MediatR;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Commands.ApprovePurchaseOrder;

public class ApprovePurchaseOrderCommand : IRequest<ApprovePurchaseOrderResponse>
{
    public int PurchaseOrderID { get; set; }
}
