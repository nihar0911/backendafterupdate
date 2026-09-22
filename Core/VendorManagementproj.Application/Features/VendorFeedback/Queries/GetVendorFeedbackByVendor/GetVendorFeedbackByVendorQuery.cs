using MediatR;

namespace VendorManagementprojApplication.Features.VendorFeedback.Queries.GetVendorFeedbackByVendor;

public class GetVendorFeedbackByVendorQuery : IRequest<GetVendorFeedbackByVendorResponse>
{
    public int VendorID { get; set; }
    public int? ProductID { get; set; }
}
