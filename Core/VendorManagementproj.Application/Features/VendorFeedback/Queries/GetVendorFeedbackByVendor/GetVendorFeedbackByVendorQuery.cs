using MediatR;

namespace VendorManagementproj.Application.Features.VendorFeedback.Queries.GetVendorFeedbackByVendor;

public class GetVendorFeedbackByVendorQuery : IRequest<GetVendorFeedbackByVendorResponse>
{
    public int VendorID { get; set; }
    public int? ProductID { get; set; }
}
