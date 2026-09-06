using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.VendorFeedback.Queries.GetVendorFeedbackById;

public class GetVendorFeedbackByIdResponse
{
    public VendorFeedbackDto? Feedback { get; set; }
}