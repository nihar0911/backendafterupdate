using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderResponse
{
    public PurchaseOrderDto PurchaseOrder { get; set; } = null!;
}