using System;
using System.Collections.Generic;
using MediatR;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Commands.DispatchPurchaseRequest;

public class ItemVendorAssignmentDto
{
    public int? RequestItemID { get; set; }
    public int ProductID { get; set; }
    public int VendorID { get; set; }
}

public class DispatchPurchaseRequestCommand : IRequest<DispatchPurchaseRequestResponse>
{
    public int RequestID { get; set; }
    public List<int> SelectedVendorIDs { get; set; } = new();
    public List<ItemVendorAssignmentDto>? ItemVendorAssignments { get; set; }
}

public class DispatchPurchaseRequestResponse
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public int OpportunitiesCreated { get; set; }
    public int NotificationsSent { get; set; }
}
