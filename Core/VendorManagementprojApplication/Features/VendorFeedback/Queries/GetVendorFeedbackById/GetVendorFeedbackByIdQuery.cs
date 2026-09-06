using MediatR;

namespace VendorManagementprojApplication.Features.VendorFeedback.Queries.GetVendorFeedbackById;

public class GetVendorFeedbackByIdQuery : IRequest<GetVendorFeedbackByIdResponse>
{
    public int FeedbackID { get; set; }
}