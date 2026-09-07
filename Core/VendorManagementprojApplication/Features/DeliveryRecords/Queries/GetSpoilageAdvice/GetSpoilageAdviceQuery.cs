using MediatR;

namespace VendorManagementprojApplication.Features.DeliveryRecords.Queries.GetSpoilageAdvice;

public class GetSpoilageAdviceQuery : IRequest<GetSpoilageAdviceResponse>
{
    public int PurchaseOrderID { get; set; }
}
