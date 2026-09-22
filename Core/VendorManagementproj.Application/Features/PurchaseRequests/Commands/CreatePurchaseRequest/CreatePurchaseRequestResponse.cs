using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.PurchaseRequests.Commands.CreatePurchaseRequest;

public class CreatePurchaseRequestResponse
{
    public PurchaseRequestDto PurchaseRequest { get; set; } = null!;
}
