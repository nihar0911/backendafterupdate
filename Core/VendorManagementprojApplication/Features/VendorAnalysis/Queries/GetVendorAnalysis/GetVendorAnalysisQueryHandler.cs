using MediatR;
using VendorManagementprojApplication.Contracts.Services;

namespace VendorManagementprojApplication.Features.VendorAnalysis.Queries.GetVendorAnalysis;

public class GetVendorAnalysisQueryHandler
    : IRequestHandler<GetVendorAnalysisQuery, GetVendorAnalysisResponse>
{
    private readonly IVendorAnalysisService _vendorAnalysisService;

    public GetVendorAnalysisQueryHandler(
        IVendorAnalysisService vendorAnalysisService)
    {
        _vendorAnalysisService = vendorAnalysisService;
    }

    public async Task<GetVendorAnalysisResponse> Handle(
        GetVendorAnalysisQuery request,
        CancellationToken cancellationToken)
    {
        var analysis =
            await _vendorAnalysisService.AnalyzeVendorAsync(
                request.VendorID,
                request.ProductID,
                request.OutletID);

        return new GetVendorAnalysisResponse
        {
            Analysis = analysis
        };
    }
}