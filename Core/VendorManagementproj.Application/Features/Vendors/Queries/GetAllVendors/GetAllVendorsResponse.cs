using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Vendors.Queries.GetAllVendors;

public class GetAllVendorsResponse
{
    public List<VendorDto> Vendors { get; set; } = new();
}
