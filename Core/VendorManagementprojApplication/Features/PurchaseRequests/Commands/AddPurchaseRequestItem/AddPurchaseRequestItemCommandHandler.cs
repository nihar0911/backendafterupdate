using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Commands.AddPurchaseRequestItem;

public class AddPurchaseRequestItemCommandHandler
    : IRequestHandler<AddPurchaseRequestItemCommand, AddPurchaseRequestItemResponse>
{
    private readonly IPurchaseRequestRepository _repository;
    private readonly IProductRepository _productRepository;
    private readonly ICurrentUserService _currentUserService;

    public AddPurchaseRequestItemCommandHandler(
        IPurchaseRequestRepository repository,
        IProductRepository productRepository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _productRepository = productRepository;
        _currentUserService = currentUserService;
    }

    public async Task<AddPurchaseRequestItemResponse> Handle(
        AddPurchaseRequestItemCommand request,
        CancellationToken cancellationToken)
    {
        if (request.RequestID <= 0)
            throw new InvalidOperationException("A valid purchase request is required.");

        if (request.ProductID <= 0)
            throw new InvalidOperationException("A valid product is required.");

        if (request.Quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.Unit))
            throw new InvalidOperationException("Unit is required.");

        var purchaseRequest = await _repository.GetByIdAsync(request.RequestID);

        if (purchaseRequest == null)
            return new AddPurchaseRequestItemResponse
            {
                Item = null
            };

        if (!string.Equals(
            purchaseRequest.Status,
            "Pending",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Items can only be added to a pending purchase request.");
        }

        bool isAuthorized = false;
        if (_currentUserService.IsAdmin)
        {
            isAuthorized = true;
        }
        else if (_currentUserService.IsPurchaseManager && _currentUserService.OutletID.HasValue)
        {
            if (_currentUserService.OutletID.Value == purchaseRequest.OutletID)
            {
                isAuthorized = true;
            }
        }

        if (!isAuthorized)
        {
            throw new UnauthorizedAccessException("User is not authorized to add items to this purchase request.");
        }

        var product = await _productRepository.GetByIdAsync(request.ProductID);

        if (product == null)
            throw new InvalidOperationException($"ProductID {request.ProductID} does not exist.");

        var item = new PurchaseRequestItem
        {
            RequestID = request.RequestID,
            ProductID = request.ProductID,
            Quantity = request.Quantity,
            Unit = request.Unit
        };

        var saved = await _repository.AddItemAsync(item);

        var dto = new PurchaseRequestItemDto
        {
            RequestItemID = saved.RequestItemID,
            RequestID = saved.RequestID,
            ProductID = saved.ProductID,
            ProductName = saved.Product?.ProductName ?? product.ProductName,
            Quantity = saved.Quantity,
            Unit = saved.Unit
        };

        return new AddPurchaseRequestItemResponse
        {
            Item = dto
        };
    }
}