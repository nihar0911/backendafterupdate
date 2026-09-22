using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.DeliveryRecords.Queries.GetDeliveriesByPurchaseOrder;

public class GetDeliveriesByPurchaseOrderResponse
{
    public List<DeliveryRecordDto> Deliveries { get; set; } = new();
}