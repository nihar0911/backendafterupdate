using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;

namespace VendorManagementprojApplication.Features.Vendors.Commands.DeleteVendor;

public class DeleteVendorCommandHandler : IRequestHandler<DeleteVendorCommand, DeleteVendorResponse>
{
    private readonly IVendorRepository _vendorRepository;

    public DeleteVendorCommandHandler(IVendorRepository vendorRepository)
    {
        _vendorRepository = vendorRepository;
    }

    public async Task<DeleteVendorResponse> Handle(DeleteVendorCommand request, CancellationToken cancellationToken)
    {
        var success = await _vendorRepository.DeleteAsync(request.VendorID);
        return new DeleteVendorResponse
        {
            Success = success
        };
    }
}
