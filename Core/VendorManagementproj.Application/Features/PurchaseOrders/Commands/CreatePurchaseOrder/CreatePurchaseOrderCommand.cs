using MediatR;

namespace VendorManagementproj.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderCommand : IRequest<CreatePurchaseOrderResponse>
{
    public int QuotationID { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? ApproverRole { get; set; }
}