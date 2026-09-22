using MediatR;

namespace VendorManagementproj.Application.Features.PurchaseOrders.Queries.GetPurchaseOrderById;

public class GetPurchaseOrderByIdQuery : IRequest<GetPurchaseOrderByIdResponse>
{
    public int PurchaseOrderID { get; set; }
}