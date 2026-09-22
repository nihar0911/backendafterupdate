using MediatR;

namespace VendorManagementproj.Application.Features.DeliveryRecords.Queries.GetSpoilageAdvice;

public class GetSpoilageAdviceQuery : IRequest<GetSpoilageAdviceResponse>
{
    public int PurchaseOrderID { get; set; }
}
