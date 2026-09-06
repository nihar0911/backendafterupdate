using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Complaints.Queries.GetAllComplaints;

public class GetAllComplaintsResponse
{
    public List<ComplaintDto> Complaints { get; set; } = new();
}