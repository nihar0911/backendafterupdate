using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Outlets.Queries.GetOutletsByOrganizationId;

public class GetOutletsByOrganizationIdResponse
{
    public List<OutletDto> Outlets { get; set; } = new();
}
