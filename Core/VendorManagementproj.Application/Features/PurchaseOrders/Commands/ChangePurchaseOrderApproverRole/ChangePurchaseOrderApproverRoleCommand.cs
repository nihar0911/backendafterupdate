using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.PurchaseOrders.Commands.ChangePurchaseOrderApproverRole;

public class ChangePurchaseOrderApproverRoleCommand : IRequest<ChangePurchaseOrderApproverRoleResponse>
{
    public int PurchaseOrderID { get; set; }
    public string ApproverRole { get; set; } = string.Empty;
}
