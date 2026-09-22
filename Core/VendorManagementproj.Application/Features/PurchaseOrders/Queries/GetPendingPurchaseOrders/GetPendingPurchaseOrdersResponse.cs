using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.PurchaseOrders.Queries.GetPendingPurchaseOrders;

public class GetPendingPurchaseOrdersResponse
{
    public List<PurchaseOrderDto> PurchaseOrders { get; set; } = new();
}