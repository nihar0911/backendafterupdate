using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.PurchaseRequests.Commands.AddPurchaseRequestItem;

public class AddPurchaseRequestItemCommand : IRequest<AddPurchaseRequestItemResponse>
{
    public int RequestID { get; set; }

    public int ProductID { get; set; }

    public decimal Quantity { get; set; }

    public string Unit { get; set; } = string.Empty;
}