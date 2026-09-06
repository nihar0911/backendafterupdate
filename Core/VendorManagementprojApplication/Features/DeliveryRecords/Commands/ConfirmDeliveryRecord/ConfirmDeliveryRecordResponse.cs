using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.DeliveryRecords.Commands.ConfirmDeliveryRecord;

public class ConfirmDeliveryRecordResponse
{
    public DeliveryRecordDto DeliveryRecord { get; set; } = null!;
}