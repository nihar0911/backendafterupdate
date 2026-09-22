using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Queries.GetPendingPurchaseOrders;

public class GetPendingPurchaseOrdersResponse
{
    public List<PurchaseOrderDto> PurchaseOrders { get; set; } = new();
}