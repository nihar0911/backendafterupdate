using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Commands.ApprovePurchaseOrder;

public class ApprovePurchaseOrderResponse
{
    public PurchaseOrderDto PurchaseOrder { get; set; } = null!;
}
