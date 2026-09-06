using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.VendorProducts.Queries.GetVendorProductById;

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
            VendorProduct = item
        };
    }
}
