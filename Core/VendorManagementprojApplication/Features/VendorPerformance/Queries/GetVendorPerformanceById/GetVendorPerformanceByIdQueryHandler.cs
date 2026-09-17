using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Services;

namespace VendorManagementprojApplication.Features.VendorPerformance.Queries.GetVendorPerformanceById;

public class GetVendorPerformanceByIdQueryHandler
    : IRequestHandler<GetVendorPerformanceByIdQuery, GetVendorPerformanceByIdResponse>
{
    private readonly IVendorPerformanceService _performanceService;

    public GetVendorPerformanceByIdQueryHandler(IVendorPerformanceService performanceService)
    {
        _performanceService = performanceService;
    }

    public async Task<GetVendorPerformanceByIdResponse> Handle(
        GetVendorPerformanceByIdQuery request,
        CancellationToken cancellationToken)
    {
        var summary = await _performanceService.GetVendorPerformanceAsync(request.VendorID, request.OrganizationID);
        return new GetVendorPerformanceByIdResponse
        {
            VendorPerformance = summary
        };
    }
}
