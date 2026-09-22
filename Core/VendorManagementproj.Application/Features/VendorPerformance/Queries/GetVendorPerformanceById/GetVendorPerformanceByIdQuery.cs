using MediatR;

namespace VendorManagementproj.Application.Features.VendorPerformance.Queries.GetVendorPerformanceById;

public class GetVendorPerformanceByIdQuery : IRequest<GetVendorPerformanceByIdResponse>
{
    public int VendorID { get; set; }
    public int? OrganizationID { get; set; }
}
