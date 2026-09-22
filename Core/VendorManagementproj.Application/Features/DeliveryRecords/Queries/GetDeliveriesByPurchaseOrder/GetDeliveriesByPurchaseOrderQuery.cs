using MediatR;
using VendorManagementproj.Application.Features.DeliveryRecords.Queries.GetDeliveriesByPurchaseOrder;

namespace VendorManagementproj.Application.Features.DeliveryRecords.Queries.GetDeliveriesByPurchaseOrder;

public class GetDeliveriesByPurchaseOrderQuery
    : IRequest<GetDeliveriesByPurchaseOrderResponse>
{
    public int PurchaseOrderID { get; set; }
}