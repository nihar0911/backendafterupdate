using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.VendorProducts.Queries.GetAllVendorProducts;

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
            VendorProducts = list
        };
    }
}
