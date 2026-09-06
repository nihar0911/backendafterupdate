using MediatR;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Commands.RespondToPurchaseOrder;

public class RespondToPurchaseOrderCommand : IRequest<RespondToPurchaseOrderResponse>
{
    public int PurchaseOrderID { get; set; }
    public int VendorID { get; set; }
    public string Status { get; set; } = string.Empty;
}