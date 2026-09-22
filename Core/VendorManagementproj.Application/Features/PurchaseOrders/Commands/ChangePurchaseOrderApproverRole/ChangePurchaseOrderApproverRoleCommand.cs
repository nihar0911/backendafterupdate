using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Commands.ChangePurchaseOrderApproverRole;

public class ChangePurchaseOrderApproverRoleCommand : IRequest<ChangePurchaseOrderApproverRoleResponse>
{
    public int PurchaseOrderID { get; set; }
    public string ApproverRole { get; set; } = string.Empty;
}
