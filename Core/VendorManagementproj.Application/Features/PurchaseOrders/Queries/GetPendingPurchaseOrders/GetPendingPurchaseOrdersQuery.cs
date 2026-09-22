using MediatR;

namespace VendorManagementproj.Application.Features.PurchaseOrders.Queries.GetPendingPurchaseOrders;

public class GetPendingPurchaseOrdersQuery : IRequest<GetPendingPurchaseOrdersResponse>
{
    public int VendorID { get; set; }
}