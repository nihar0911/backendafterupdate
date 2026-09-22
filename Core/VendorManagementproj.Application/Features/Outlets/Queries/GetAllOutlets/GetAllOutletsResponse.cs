using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Outlets.Queries.GetAllOutlets;

public class GetAllOutletsResponse
{
    public List<OutletDto> Outlets { get; set; } = new();
}
