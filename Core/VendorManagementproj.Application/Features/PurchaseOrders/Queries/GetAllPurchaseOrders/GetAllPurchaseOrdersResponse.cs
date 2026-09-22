using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Queries.GetAllPurchaseOrders;

public class GetAllPurchaseOrdersResponse
{
    public List<PurchaseOrderDto> PurchaseOrders { get; set; } = new();
}