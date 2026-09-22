using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.VendorProducts.Queries.GetVendorProductsByProductName;

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
            VendorProducts = vendorProducts.Select(vp => new VendorProductDto
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
