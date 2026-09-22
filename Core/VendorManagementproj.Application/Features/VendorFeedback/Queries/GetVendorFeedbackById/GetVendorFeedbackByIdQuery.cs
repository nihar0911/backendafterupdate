using MediatR;

namespace VendorManagementproj.Application.Features.VendorFeedback.Queries.GetVendorFeedbackById;

public class GetVendorFeedbackByIdQuery : IRequest<GetVendorFeedbackByIdResponse>
{
    public int FeedbackID { get; set; }
}