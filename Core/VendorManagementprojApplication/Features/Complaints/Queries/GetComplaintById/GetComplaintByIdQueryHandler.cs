using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Complaints.Queries.GetComplaintById;

public class GetComplaintByIdQueryHandler
    : IRequestHandler<GetComplaintByIdQuery, GetComplaintByIdResponse>
{
    private readonly IComplaintRepository _complaintRepository;

    public GetComplaintByIdQueryHandler(
        IComplaintRepository complaintRepository)
    {
        _complaintRepository = complaintRepository;
    }

    public async Task<GetComplaintByIdResponse> Handle(
        GetComplaintByIdQuery request,
        CancellationToken cancellationToken)
    {
        var complaint =
            await _complaintRepository
                .GetByIdAsync(request.ComplaintID);

        if (complaint == null)
        {
            return new GetComplaintByIdResponse
            {
                Complaint = null
            };
        }

        return new GetComplaintByIdResponse
        {
            Complaint = new ComplaintDto
            {
                ComplaintID = complaint.ComplaintID,
                VendorID = complaint.VendorID,
                OutletID = complaint.OutletID,
                PurchaseOrderID = complaint.PurchaseOrderID,
                POItemID = complaint.POItemID,
                ProductID = complaint.ProductID,
                RaisedByUserID = complaint.RaisedByUserID,
                ComplaintDate = complaint.ComplaintDate,
                ComplaintType = complaint.ComplaintType,
                Description = complaint.Description,
                Severity = complaint.Severity,
                Status = complaint.Status
            }
        };
    }
}