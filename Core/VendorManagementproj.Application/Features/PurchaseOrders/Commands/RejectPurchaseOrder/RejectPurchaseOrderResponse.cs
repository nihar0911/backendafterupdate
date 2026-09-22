using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Commands.RejectPurchaseOrder;

public class RejectPurchaseOrderResponse
{
    public PurchaseOrderDto PurchaseOrder { get; set; } = null!;
}
