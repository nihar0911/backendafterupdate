using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.PurchaseRequests.Queries.GetPurchaseRequestItems;

public record GetPurchaseRequestItemsQuery(
    int RequestID
) : IRequest<GetPurchaseRequestItemsResponse>;