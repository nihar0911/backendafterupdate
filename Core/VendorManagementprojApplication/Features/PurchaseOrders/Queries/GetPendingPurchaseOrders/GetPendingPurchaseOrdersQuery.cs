using MediatR;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Queries.GetPendingPurchaseOrders;

public class GetPendingPurchaseOrdersQuery : IRequest<GetPendingPurchaseOrdersResponse>
{
    public int VendorID { get; set; }
}