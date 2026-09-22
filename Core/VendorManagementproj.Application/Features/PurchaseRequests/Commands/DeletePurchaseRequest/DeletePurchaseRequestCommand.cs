using MediatR;

namespace VendorManagementproj.Application.Features.PurchaseRequests.Commands.DeletePurchaseRequest;

public record DeletePurchaseRequestCommand(int RequestID) : IRequest<DeletePurchaseRequestResponse>;
