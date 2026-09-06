using MediatR;

namespace VendorManagementprojApplication.Features.Complaints.Queries.GetComplaintById;

public class GetComplaintByIdQuery : IRequest<GetComplaintByIdResponse>
{
    public int ComplaintID { get; set; }
}