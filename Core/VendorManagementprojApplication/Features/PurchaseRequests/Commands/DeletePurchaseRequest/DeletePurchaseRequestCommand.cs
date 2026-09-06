using MediatR;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Commands.DeletePurchaseRequest;

public record DeletePurchaseRequestCommand(int RequestID) : IRequest<DeletePurchaseRequestResponse>;
