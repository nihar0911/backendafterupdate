using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.PurchaseRequests.Queries.GetAllPurchaseRequests;

public class GetAllPurchaseRequestsResponse
{
    public List<PurchaseRequestDto> PurchaseRequests { get; set; } = new();
}
