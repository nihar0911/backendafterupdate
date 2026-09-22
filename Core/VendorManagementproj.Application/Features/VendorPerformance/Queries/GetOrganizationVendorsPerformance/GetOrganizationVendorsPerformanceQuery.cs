using MediatR;

namespace VendorManagementproj.Application.Features.VendorPerformance.Queries.GetOrganizationVendorsPerformance;

public class GetOrganizationVendorsPerformanceQuery : IRequest<GetOrganizationVendorsPerformanceResponse>
{
    public int? OrganizationID { get; set; }
}
