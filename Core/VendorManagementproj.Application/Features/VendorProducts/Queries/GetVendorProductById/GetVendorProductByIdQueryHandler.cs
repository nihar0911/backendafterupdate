using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.VendorProducts.Queries.GetVendorProductById;

public class GetVendorProductByIdQueryHandler : IRequestHandler<GetVendorProductByIdQuery, GetVendorProductByIdResponse>
{
    private readonly IVendorProductRepository _repository;

    public GetVendorProductByIdQueryHandler(IVendorProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetVendorProductByIdResponse> Handle(GetVendorProductByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.VendorProductID);
        return new GetVendorProductByIdResponse
        {
            VendorProduct = item == null ? null : new VendorProductDto
            {
                VendorProductID = item.VendorProductID,
                VendorID = item.VendorID,
                ProductID = item.ProductID,
                UnitPrice = item.UnitPrice,
                EstimatedDeliveryDays = item.EstimatedDeliveryDays,
                Status = item.Status
            }
        };
    }
}
