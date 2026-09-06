using System;
using System.Collections.Generic;
using MediatR;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Commands.DispatchPurchaseRequest;

public class DispatchPurchaseRequestCommand : IRequest<DispatchPurchaseRequestResponse>
{
    public int RequestID { get; set; }
    public List<int> SelectedVendorIDs { get; set; } = new();
}

public class DispatchPurchaseRequestResponse
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public int OpportunitiesCreated { get; set; }
    public int NotificationsSent { get; set; }
}
