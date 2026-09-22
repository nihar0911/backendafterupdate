using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Vendors.Commands.UpdateVendor;

public class UpdateVendorCommandHandler
    : IRequestHandler<UpdateVendorCommand, UpdateVendorResponse>
{
    private readonly IVendorRepository _vendorRepository;

    public UpdateVendorCommandHandler(
        IVendorRepository vendorRepository)
    {
        _vendorRepository = vendorRepository;
    }

    public async Task<UpdateVendorResponse> Handle(
        UpdateVendorCommand request,
        CancellationToken cancellationToken)
    {
        var vendor = new Vendor
        {
            VendorName = request.VendorName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            GSTIN = request.GSTIN,
            Status = request.Status
        };

        var updatedVendor =
            await _vendorRepository.UpdateAsync(
                request.VendorID,
                vendor);

        if (updatedVendor == null)
            return new UpdateVendorResponse { Vendor = null };

        return new UpdateVendorResponse
        {
            Vendor = new VendorDto
            {
                VendorID = updatedVendor.VendorID,
                VendorName = updatedVendor.VendorName,
                Email = updatedVendor.Email,
                Phone = updatedVendor.Phone,
                Address = updatedVendor.Address,
                Latitude = updatedVendor.Latitude,
                Longitude = updatedVendor.Longitude,
                GSTIN = updatedVendor.GSTIN,
                Status = updatedVendor.Status
            }
        };
    }
}