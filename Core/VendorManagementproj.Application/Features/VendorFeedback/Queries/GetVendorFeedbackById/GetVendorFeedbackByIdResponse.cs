using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.VendorFeedback.Queries.GetVendorFeedbackById;

public class GetVendorFeedbackByIdResponse
{
    public VendorFeedbackDto? Feedback { get; set; }
}