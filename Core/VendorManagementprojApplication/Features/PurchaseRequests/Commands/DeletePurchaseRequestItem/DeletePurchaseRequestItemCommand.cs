using MediatR;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Commands.DeletePurchaseRequestItem;

public record DeletePurchaseRequestItemCommand(int RequestItemID) : IRequest<DeletePurchaseRequestItemResponse>;

