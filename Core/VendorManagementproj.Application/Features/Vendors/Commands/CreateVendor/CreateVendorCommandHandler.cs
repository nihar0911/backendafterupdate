using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Vendors.Commands.CreateVendor;

public class CreateVendorCommandHandler
    : IRequestHandler<CreateVendorCommand, CreateVendorResponse>
{
    private readonly IVendorRepository _vendorRepository;

    public CreateVendorCommandHandler(
        IVendorRepository vendorRepository)
    {
        _vendorRepository = vendorRepository;
    }

    public async Task<CreateVendorResponse> Handle(
        CreateVendorCommand request,
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
            Status = string.IsNullOrWhiteSpace(request.Status)
                ? "Active"
                : request.Status
        };

        var createdVendor =
            await _vendorRepository.AddAsync(vendor);

        var dto = new VendorDto
        {
            VendorID = createdVendor.VendorID,
            VendorName = createdVendor.VendorName,
            Email = createdVendor.Email,
            Phone = createdVendor.Phone,
            Address = createdVendor.Address,
            Latitude = createdVendor.Latitude,
            Longitude = createdVendor.Longitude,
            GSTIN = createdVendor.GSTIN,
            Status = createdVendor.Status
        };

        return new CreateVendorResponse
        {
            Vendor = dto
        };
    }
}