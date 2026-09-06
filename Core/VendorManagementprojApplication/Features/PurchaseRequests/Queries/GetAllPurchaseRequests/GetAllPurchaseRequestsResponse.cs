using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Queries.GetAllPurchaseRequests;

public class GetAllPurchaseRequestsResponse
{
    public List<PurchaseRequestDto> PurchaseRequests { get; set; } = new();
}
