using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.DeliveryRecords.Commands.CreateDeliveryRecord;

public class CreateDeliveryRecordResponse
{
    public DeliveryRecordDto DeliveryRecord { get; set; } = null!;
}