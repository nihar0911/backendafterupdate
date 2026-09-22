using MediatR;

namespace VendorManagementprojApplication.Features.DeliveryRecords.Commands.ConfirmDeliveryRecord;

public class ConfirmDeliveryRecordCommand
    : IRequest<ConfirmDeliveryRecordResponse>
{
    public int DeliveryRecordID { get; set; }

    public int ConfirmedByUserID { get; set; }
}