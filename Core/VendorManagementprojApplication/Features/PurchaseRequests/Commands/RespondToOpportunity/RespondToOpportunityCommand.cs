using MediatR;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Commands.RespondToOpportunity;

public class RespondToOpportunityCommand : IRequest<RespondToOpportunityResponse>
{
    public int RequestID { get; set; }
    public int ProductID { get; set; }
    public string Action { get; set; } = string.Empty; // "Accept" or "Reject"
    public string? RejectionReason { get; set; }
}
