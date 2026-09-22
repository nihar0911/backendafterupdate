using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;

namespace VendorManagementproj.Application.Features.VendorProducts.Commands.DeleteVendorProduct;

public class DeleteVendorProductCommandHandler : IRequestHandler<DeleteVendorProductCommand, DeleteVendorProductResponse>
{
    private readonly IVendorProductRepository _repository;

    public DeleteVendorProductCommandHandler(IVendorProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<DeleteVendorProductResponse> Handle(DeleteVendorProductCommand request, CancellationToken cancellationToken)
    {
        var success = await _repository.DeleteAsync(request.VendorProductID);
        return new DeleteVendorProductResponse
        {
            Success = success
        };
    }
}
