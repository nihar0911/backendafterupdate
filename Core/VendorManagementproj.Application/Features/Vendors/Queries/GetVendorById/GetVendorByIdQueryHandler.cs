using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Vendors.Queries.GetVendorById;

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
                Latitude = vendor.Latitude,
                Longitude = vendor.Longitude,
                GSTIN = vendor.GSTIN,
                Status = vendor.Status
            }
        };
    }
}
