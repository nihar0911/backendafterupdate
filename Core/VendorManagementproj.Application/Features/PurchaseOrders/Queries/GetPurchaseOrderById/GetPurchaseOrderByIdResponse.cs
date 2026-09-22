using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.PurchaseOrders.Queries.GetPurchaseOrderById;

public class GetPurchaseOrderByIdResponse
{
    public PurchaseOrderDto? PurchaseOrder { get; set; }
}
