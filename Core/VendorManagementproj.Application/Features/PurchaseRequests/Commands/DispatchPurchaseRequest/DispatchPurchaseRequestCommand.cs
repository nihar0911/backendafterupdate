using System;
using System.Collections.Generic;
using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.PurchaseRequests.Commands.DispatchPurchaseRequest;

public class DispatchPurchaseRequestCommand : IRequest<DispatchPurchaseRequestResponse>
{
    public int RequestID { get; set; }
    public List<int> SelectedVendorIDs { get; set; } = new();
    public List<ItemVendorAssignmentDto>? ItemVendorAssignments { get; set; }
}
