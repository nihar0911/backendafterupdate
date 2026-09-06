using MediatR;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Commands.SendPurchaseOrder;

public class SendPurchaseOrderCommand : IRequest<SendPurchaseOrderResponse>
{
    public int PurchaseOrderID { get; set; }
}
