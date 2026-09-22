using MediatR;

namespace VendorManagementproj.Application.Features.PurchaseOrders.Commands.SendPurchaseOrder;

public class SendPurchaseOrderCommand : IRequest<SendPurchaseOrderResponse>
{
    public int PurchaseOrderID { get; set; }
}
