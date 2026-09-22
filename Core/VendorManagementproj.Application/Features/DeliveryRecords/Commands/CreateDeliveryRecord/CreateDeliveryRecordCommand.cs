using System;
using MediatR;

namespace VendorManagementprojApplication.Features.DeliveryRecords.Commands.CreateDeliveryRecord;

public class CreateDeliveryRecordCommand
    : IRequest<CreateDeliveryRecordResponse>
{
    public int PurchaseOrderID { get; set; }

    public int POItemID { get; set; }

    public decimal ReceivedQuantity { get; set; }

    public decimal SpoiledQuantity { get; set; }

    public DateTime? DeliveryDate { get; set; }
}