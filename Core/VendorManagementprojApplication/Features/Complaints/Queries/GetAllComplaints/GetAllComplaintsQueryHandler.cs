using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Complaints.Queries.GetAllComplaints;

public class GetAllComplaintsQueryHandler
    : IRequestHandler<GetAllComplaintsQuery, GetAllComplaintsResponse>
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetAllComplaintsQueryHandler(
        IComplaintRepository complaintRepository,
        ICurrentUserService currentUserService)
    {
        _complaintRepository = complaintRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetAllComplaintsResponse> Handle(
        GetAllComplaintsQuery request,
        CancellationToken cancellationToken)
    {
        var complaints =
            await _complaintRepository.GetAllAsync();

        if (_currentUserService.IsOrganizationManager)
        {
            if (_currentUserService.OrganizationID.HasValue)
            {
                complaints = complaints.Where(c => c.Outlet != null && c.Outlet.OrganizationID == _currentUserService.OrganizationID.Value).ToList();
            }
            else
            {
                complaints = new List<Complaint>();
            }
        }
        else if (_currentUserService.IsOutletManager)
        {
            if (_currentUserService.OutletID.HasValue)
            {
                complaints = complaints.Where(c => c.OutletID == _currentUserService.OutletID.Value).ToList();
            }
            else
            {
                complaints = new List<Complaint>();
            }
        }

        return new GetAllComplaintsResponse
        {
            Complaints = complaints.Select(c => new ComplaintDto
            {
                ComplaintID = c.ComplaintID,
                VendorID = c.VendorID,
                OutletID = c.OutletID,
                PurchaseOrderID = c.PurchaseOrderID,
                POItemID = c.POItemID,
                ProductID = c.ProductID,
                RaisedByUserID = c.RaisedByUserID,
                ComplaintDate = c.ComplaintDate,
                ComplaintType = c.ComplaintType,
                Description = c.Description,
                Severity = c.Severity,
                Status = c.Status
            }).ToList()
        };
    }
}