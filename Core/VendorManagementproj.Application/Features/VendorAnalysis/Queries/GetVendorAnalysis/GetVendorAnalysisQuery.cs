using MediatR;

namespace VendorManagementproj.Application.Features.VendorAnalysis.Queries.GetVendorAnalysis;

public class GetVendorAnalysisQuery: IRequest<GetVendorAnalysisResponse>
{
    public int VendorID { get; set; }

    public int ProductID { get; set; }

    public int OutletID { get; set; }
}