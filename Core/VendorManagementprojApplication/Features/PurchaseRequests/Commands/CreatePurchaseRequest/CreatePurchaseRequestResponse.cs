using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Commands.CreatePurchaseRequest;

public class CreatePurchaseRequestResponse
{
    public PurchaseRequestDto PurchaseRequest { get; set; } = null!;
}
