using MediatR;

namespace VendorManagementproj.Application.Features.PurchaseOrders.Commands.RespondToPurchaseOrder;

public class RespondToPurchaseOrderCommand : IRequest<RespondToPurchaseOrderResponse>
{
    public int PurchaseOrderID { get; set; }
    public int VendorID { get; set; }
    public string Status { get; set; } = string.Empty;
}