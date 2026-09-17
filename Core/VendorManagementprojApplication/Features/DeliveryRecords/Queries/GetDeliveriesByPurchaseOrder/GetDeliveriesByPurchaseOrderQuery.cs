using MediatR;
using VendorManagementprojApplication.Features.DeliveryRecords.Queries.GetDeliveriesByPurchaseOrder;

namespace VendorManagementprojApplication.Features.DeliveryRecords.Queries.GetDeliveriesByPurchaseOrder;

public class GetDeliveriesByPurchaseOrderQuery
    : IRequest<GetDeliveriesByPurchaseOrderResponse>
{
    public int PurchaseOrderID { get; set; }
}