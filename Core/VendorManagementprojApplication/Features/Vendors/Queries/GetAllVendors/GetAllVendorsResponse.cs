using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Vendors.Queries.GetAllVendors;

public class GetAllVendorsResponse
{
    public List<VendorDto> Vendors { get; set; } = new();
}
