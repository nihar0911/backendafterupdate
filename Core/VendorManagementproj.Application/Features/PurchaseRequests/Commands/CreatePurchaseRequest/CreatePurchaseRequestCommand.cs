using System;
using System.Collections.Generic;
using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Commands.CreatePurchaseRequest;

public class CreatePurchaseRequestCommand : IRequest<CreatePurchaseRequestResponse>
{
    public int OutletID { get; set; }

    public int CreatedByUserID { get; set; }

    public DateTime RequestDate { get; set; }

    public List<int>? SelectedVendorIDs { get; set; }

    public int? SelectedVendorID { get; set; }

    public List<CreatePurchaseRequestItemDto> Items { get; set; } = new();
}
