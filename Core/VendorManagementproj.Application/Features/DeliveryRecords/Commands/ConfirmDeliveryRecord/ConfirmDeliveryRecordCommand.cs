using MediatR;

namespace VendorManagementproj.Application.Features.DeliveryRecords.Commands.ConfirmDeliveryRecord;

public class ConfirmDeliveryRecordCommand
    : IRequest<ConfirmDeliveryRecordResponse>
{
    public int DeliveryRecordID { get; set; }

    public int ConfirmedByUserID { get; set; }
}