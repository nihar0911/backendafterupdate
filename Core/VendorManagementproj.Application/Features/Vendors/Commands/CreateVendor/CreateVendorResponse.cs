using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Vendors.Commands.CreateVendor;

public class CreateVendorResponse
{
    public VendorDto Vendor { get; set; } = null!;
}
