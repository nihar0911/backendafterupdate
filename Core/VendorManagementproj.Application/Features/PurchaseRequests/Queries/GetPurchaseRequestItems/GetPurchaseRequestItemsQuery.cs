using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Queries.GetPurchaseRequestItems;

public record GetPurchaseRequestItemsQuery(
    int RequestID
) : IRequest<GetPurchaseRequestItemsResponse>;