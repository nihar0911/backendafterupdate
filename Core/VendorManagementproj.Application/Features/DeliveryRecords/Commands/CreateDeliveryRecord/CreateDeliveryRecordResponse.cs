using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.DeliveryRecords.Commands.CreateDeliveryRecord;

public class CreateDeliveryRecordResponse
{
    public DeliveryRecordDto DeliveryRecord { get; set; } = null!;
}