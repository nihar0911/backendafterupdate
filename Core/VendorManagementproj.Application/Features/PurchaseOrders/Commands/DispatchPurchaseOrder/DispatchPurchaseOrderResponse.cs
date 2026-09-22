using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.PurchaseOrders.Commands.DispatchPurchaseOrder;

public class DispatchPurchaseOrderResponse
{
    public PurchaseOrderDto PurchaseOrder { get; set; } = null!;
}