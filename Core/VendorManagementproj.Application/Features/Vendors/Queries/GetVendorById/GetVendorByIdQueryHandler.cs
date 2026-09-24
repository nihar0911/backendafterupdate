using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Vendors.Queries.GetVendorById;

public class GetVendorByIdQueryHandler : IRequestHandler<GetVendorByIdQuery, GetVendorByIdResponse>
{
    private readonly IVendorRepository _vendorRepository;

    public GetVendorByIdQueryHandler(IVendorRepository vendorRepository)
    {
        _vendorRepository = vendorRepository;
    }

    public async Task<GetVendorByIdResponse> Handle(GetVendorByIdQuery request, CancellationToken cancellationToken)
    {
        var vendor = await _vendorRepository.GetByIdAsync(request.VendorID);

        if (vendor == null)
            return new GetVendorByIdResponse { Vendor = null };

        return new GetVendorByIdResponse
        {
            Vendor = new VendorDto
            {
                VendorID = vendor.VendorID,
                VendorName = vendor.VendorName,
                Email = vendor.Email,
                Phone = vendor.Phone,
                Address = vendor.Address,
                GSTIN = vendor.GSTIN,
                Status = vendor.Status
            }
        };
    }
}
