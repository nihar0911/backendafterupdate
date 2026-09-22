using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementproj.Application.Contracts.Services;

namespace VendorManagementproj.Application.Features.VendorPerformance.Queries.GetOrganizationVendorsPerformance;

public class GetOrganizationVendorsPerformanceQueryHandler
    : IRequestHandler<GetOrganizationVendorsPerformanceQuery, GetOrganizationVendorsPerformanceResponse>
{
    private readonly IVendorPerformanceService _performanceService;

    public GetOrganizationVendorsPerformanceQueryHandler(IVendorPerformanceService performanceService)
    {
        _performanceService = performanceService;
    }

    public async Task<GetOrganizationVendorsPerformanceResponse> Handle(
        GetOrganizationVendorsPerformanceQuery request,
        CancellationToken cancellationToken)
    {
        var list = await _performanceService.GetOrganizationVendorsPerformanceAsync(request.OrganizationID);
        return new GetOrganizationVendorsPerformanceResponse
        {
            VendorPerformances = list
        };
    }
}
