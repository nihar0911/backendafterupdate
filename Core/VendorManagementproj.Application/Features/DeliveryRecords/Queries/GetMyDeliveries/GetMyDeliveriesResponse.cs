using System.Collections.Generic;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.DeliveryRecords.Queries.GetMyDeliveries;

public class GetMyDeliveriesResponse
{
    public List<VendorDeliveryItemDto> Deliveries { get; set; } = new();
    public int TotalDeliveries { get; set; }
    public decimal TotalReceivedQuantity { get; set; }
    public decimal TotalSpoiledQuantity { get; set; }
    public int DistinctProductsCount { get; set; }
}
