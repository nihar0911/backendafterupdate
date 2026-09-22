using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.PurchaseRequests.Queries.GetPurchaseRequestById;

public record GetPurchaseRequestByIdQuery(int RequestID) : IRequest<GetPurchaseRequestByIdResponse>;
