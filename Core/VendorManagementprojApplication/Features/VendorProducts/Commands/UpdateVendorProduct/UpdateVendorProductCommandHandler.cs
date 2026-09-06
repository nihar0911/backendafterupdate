using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.VendorProducts.Commands.UpdateVendorProduct;

public class UpdateVendorProductCommandHandler : IRequestHandler<UpdateVendorProductCommand, UpdateVendorProductResponse>
{
    private readonly IVendorProductRepository _repository;

    public UpdateVendorProductCommandHandler(IVendorProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<UpdateVendorProductResponse> Handle(
        UpdateVendorProductCommand request,
        CancellationToken cancellationToken)
    {
        var vendorProduct = new VendorProduct
        {
            VendorProductID = request.VendorProductID,
            VendorID = request.VendorID,
            ProductID = request.ProductID,
            UnitPrice = request.UnitPrice,
            EstimatedDeliveryDays = request.EstimatedDeliveryDays,
            Status = request.Status
        };

        var updated = await _repository.UpdateAsync(
            request.VendorProductID,
            vendorProduct);

        if (updated == null)
        {
            return new UpdateVendorProductResponse
            {
                VendorProduct = null
            };
        }

        return new UpdateVendorProductResponse
        {
            VendorProduct = new VendorProductDto
            {
                VendorProductID = updated.VendorProductID,
                VendorID = updated.VendorID,
                ProductID = updated.ProductID,
                UnitPrice = updated.UnitPrice,
                EstimatedDeliveryDays = updated.EstimatedDeliveryDays,
                Status = updated.Status
            }
        };
    }
}