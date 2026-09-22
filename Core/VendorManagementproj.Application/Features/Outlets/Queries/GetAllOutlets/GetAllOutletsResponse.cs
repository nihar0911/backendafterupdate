using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Outlets.Queries.GetAllOutlets;

public class GetAllOutletsResponse
{
    public List<OutletDto> Outlets { get; set; } = new();
}
