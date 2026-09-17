using MediatR;

namespace VendorManagementprojApplication.Features.VendorPerformance.Queries.GetOrganizationVendorsPerformance;

public class GetOrganizationVendorsPerformanceQuery : IRequest<GetOrganizationVendorsPerformanceResponse>
{
    public int? OrganizationID { get; set; }
}
