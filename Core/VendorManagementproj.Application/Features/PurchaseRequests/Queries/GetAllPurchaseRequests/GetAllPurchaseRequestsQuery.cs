using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Queries.GetAllPurchaseRequests;

public record GetAllPurchaseRequestsQuery : IRequest<GetAllPurchaseRequestsResponse>;
