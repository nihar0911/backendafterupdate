using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Commands.SendPurchaseOrder;

public class SendPurchaseOrderResponse
{
    public PurchaseOrderDto PurchaseOrder { get; set; } = null!;
}
