using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Vendors.Commands.CreateVendor;

public class CreateVendorResponse
{
    public VendorDto Vendor { get; set; } = null!;
}
