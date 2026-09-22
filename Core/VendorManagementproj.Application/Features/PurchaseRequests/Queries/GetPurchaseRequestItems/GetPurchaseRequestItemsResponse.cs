using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.PurchaseRequests.Queries.GetPurchaseRequestItems;

public class GetPurchaseRequestItemsResponse
{
    public List<PurchaseRequestItemDto> Items { get; set; } = new();
}
