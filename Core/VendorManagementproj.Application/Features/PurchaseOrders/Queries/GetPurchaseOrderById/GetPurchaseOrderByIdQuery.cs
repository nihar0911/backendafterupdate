using MediatR;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Queries.GetPurchaseOrderById;

public class GetPurchaseOrderByIdQuery : IRequest<GetPurchaseOrderByIdResponse>
{
    public int PurchaseOrderID { get; set; }
}