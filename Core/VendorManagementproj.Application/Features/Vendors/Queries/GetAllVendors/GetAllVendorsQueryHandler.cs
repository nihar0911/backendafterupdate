using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Vendors.Queries.GetAllVendors;

public class GetAllVendorsQueryHandler
    : IRequestHandler<GetAllVendorsQuery, GetAllVendorsResponse>
{
    private readonly IVendorRepository _vendorRepository;

    public GetAllVendorsQueryHandler(
        IVendorRepository vendorRepository)
    {
        _vendorRepository = vendorRepository;
    }

    public async Task<GetAllVendorsResponse> Handle(
        GetAllVendorsQuery request,
        CancellationToken cancellationToken)
    {
        var vendors =
            await _vendorRepository.GetAllAsync();

        var list = vendors.Select(v => new VendorDto
        {
            VendorID = v.VendorID,
            VendorName = v.VendorName,
            Email = v.Email,
            Phone = v.Phone,
            Address = v.Address,
            Latitude = v.Latitude,
            Longitude = v.Longitude,
            GSTIN = v.GSTIN,
            Status = v.Status
        }).ToList();

        return new GetAllVendorsResponse
        {
            Vendors = list
        };
    }
}