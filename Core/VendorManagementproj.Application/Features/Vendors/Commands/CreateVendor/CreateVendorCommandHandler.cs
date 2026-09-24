using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.DTOs;
using VendorManagementproj.Domain.Entities;

namespace VendorManagementproj.Application.Features.Vendors.Commands.CreateVendor;

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
            GSTIN = createdVendor.GSTIN,
            Status = createdVendor.Status
        };

        return new CreateVendorResponse
        {
            Vendor = dto
        };
    }
}