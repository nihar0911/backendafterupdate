using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Queries.GetPurchaseRequestItems;

public class GetPurchaseRequestItemsResponse
{
    public List<PurchaseRequestItemDto> Items { get; set; } = new();
}
