using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.VendorFeedback.Queries.GetAllVendorFeedback;

public class GetAllVendorFeedbackResponse
{
    public List<VendorFeedbackDto> Feedback { get; set; } = new();
}