using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.VendorProducts.Queries.GetVendorProductsByProductName;

public class GetVendorProductsByProductNameQueryHandler
    : IRequestHandler<
        GetVendorProductsByProductNameQuery,
        GetVendorProductsByProductNameResponse>
{
    private readonly IVendorProductRepository _repository;

    public GetVendorProductsByProductNameQueryHandler(
        IVendorProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetVendorProductsByProductNameResponse> Handle(
        GetVendorProductsByProductNameQuery request,
        CancellationToken cancellationToken)
    {
        var vendorProducts = await _repository.GetByProductNameAsync(
            request.ProductName);

        return new GetVendorProductsByProductNameResponse
        {
            VendorProducts = vendorProducts
        };
    }
}