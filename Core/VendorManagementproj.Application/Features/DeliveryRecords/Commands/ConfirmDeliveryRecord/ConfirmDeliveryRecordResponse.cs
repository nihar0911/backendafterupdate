using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.DeliveryRecords.Commands.ConfirmDeliveryRecord;

public class ConfirmDeliveryRecordResponse
{
    public DeliveryRecordDto DeliveryRecord { get; set; } = null!;
}