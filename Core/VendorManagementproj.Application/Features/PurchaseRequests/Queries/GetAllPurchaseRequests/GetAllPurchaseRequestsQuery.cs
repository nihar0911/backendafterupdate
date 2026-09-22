using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.PurchaseRequests.Queries.GetAllPurchaseRequests;

public record GetAllPurchaseRequestsQuery : IRequest<GetAllPurchaseRequestsResponse>;
