using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Queries.GetPurchaseRequestById;

public record GetPurchaseRequestByIdQuery(int RequestID) : IRequest<GetPurchaseRequestByIdResponse>;
