using MediatR;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Queries.ParseVoiceProcurementOrder;

public class ParseVoiceProcurementOrderQuery : IRequest<ParseVoiceProcurementOrderResponse>
{
    public string Prompt { get; set; } = string.Empty;
}
