using System.Linq;
using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.VendorProducts.Queries.GetAllVendorProducts;

public class GetAllVendorProductsQueryHandler : IRequestHandler<GetAllVendorProductsQuery, GetAllVendorProductsResponse>
{
    private readonly IVendorProductRepository _repository;

    public GetAllVendorProductsQueryHandler(IVendorProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetAllVendorProductsResponse> Handle(GetAllVendorProductsQuery request, CancellationToken cancellationToken)
    {
        var list = await _repository.GetAllAsync();
        return new GetAllVendorProductsResponse
        {
            VendorProducts = list.Select(vp => new VendorProductDto
            {
                VendorProductID = vp.VendorProductID,
                VendorID = vp.VendorID,
                ProductID = vp.ProductID,
                UnitPrice = vp.UnitPrice,
                EstimatedDeliveryDays = vp.EstimatedDeliveryDays,
                Status = vp.Status
            }).ToList()
        };
    }
}
