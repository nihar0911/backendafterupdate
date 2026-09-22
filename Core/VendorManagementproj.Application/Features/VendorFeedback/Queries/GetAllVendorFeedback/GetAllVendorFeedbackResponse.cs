using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.VendorFeedback.Queries.GetAllVendorFeedback;

public class GetAllVendorFeedbackResponse
{
    public List<VendorFeedbackDto> Feedback { get; set; } = new();
}