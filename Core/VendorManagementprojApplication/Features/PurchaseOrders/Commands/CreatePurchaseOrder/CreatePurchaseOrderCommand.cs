using MediatR;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderCommand : IRequest<CreatePurchaseOrderResponse>
{
    public int QuotationID { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
}