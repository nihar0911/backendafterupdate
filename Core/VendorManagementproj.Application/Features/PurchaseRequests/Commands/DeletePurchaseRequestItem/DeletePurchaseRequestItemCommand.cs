using MediatR;

namespace VendorManagementproj.Application.Features.PurchaseRequests.Commands.DeletePurchaseRequestItem;

public record DeletePurchaseRequestItemCommand(int RequestItemID) : IRequest<DeletePurchaseRequestItemResponse>;

