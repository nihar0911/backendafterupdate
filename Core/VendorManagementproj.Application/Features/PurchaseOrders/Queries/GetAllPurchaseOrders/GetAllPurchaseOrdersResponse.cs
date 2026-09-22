using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.PurchaseOrders.Queries.GetAllPurchaseOrders;

public class GetAllPurchaseOrdersResponse
{
    public List<PurchaseOrderDto> PurchaseOrders { get; set; } = new();
}