using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.PurchaseOrders.Commands.ChangePurchaseOrderApproverRole;

public class ChangePurchaseOrderApproverRoleResponse
{
    public PurchaseOrderDto? PurchaseOrder { get; set; }
}
