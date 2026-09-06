using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.DeliveryRecords.Queries.GetDeliveriesByPurchaseOrder;

public class GetDeliveriesByPurchaseOrderResponse
{
    public List<DeliveryRecordDto> Deliveries { get; set; } = new();
}