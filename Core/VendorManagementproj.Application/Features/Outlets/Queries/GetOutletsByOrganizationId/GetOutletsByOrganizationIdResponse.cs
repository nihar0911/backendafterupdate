using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Outlets.Queries.GetOutletsByOrganizationId;

public class GetOutletsByOrganizationIdResponse
{
    public List<OutletDto> Outlets { get; set; } = new();
}
