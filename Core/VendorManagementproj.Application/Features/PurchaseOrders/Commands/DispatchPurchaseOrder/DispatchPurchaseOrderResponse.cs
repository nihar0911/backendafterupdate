using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Commands.DispatchPurchaseOrder;

public class DispatchPurchaseOrderResponse
{
    public PurchaseOrderDto PurchaseOrder { get; set; } = null!;
}