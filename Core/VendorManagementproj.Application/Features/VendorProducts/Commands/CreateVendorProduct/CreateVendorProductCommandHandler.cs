using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.DTOs;
using VendorManagementproj.Domain.Entities;

namespace VendorManagementproj.Application.Features.VendorProducts.Commands.CreateVendorProduct;

public class CreateVendorProductCommandHandler : IRequestHandler<CreateVendorProductCommand, CreateVendorProductResponse>
{
    private readonly IVendorProductRepository _repository;

    public CreateVendorProductCommandHandler(IVendorProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateVendorProductResponse> Handle(
        CreateVendorProductCommand request,
        CancellationToken cancellationToken)
    {
        var vendorProduct = new VendorProduct
        {
            VendorID = request.VendorID,
            ProductID = request.ProductID,
            UnitPrice = request.UnitPrice,
            EstimatedDeliveryDays = request.EstimatedDeliveryDays,
            Status = request.Status
        };

        var created = await _repository.AddAsync(vendorProduct);

        return new CreateVendorProductResponse
        {
            VendorProduct = new VendorProductDto
            {
                VendorProductID = created.VendorProductID,
                VendorID = created.VendorID,
                ProductID = created.ProductID,
                UnitPrice = created.UnitPrice,
                EstimatedDeliveryDays = created.EstimatedDeliveryDays,
                Status = created.Status
            }
        };
    }
}