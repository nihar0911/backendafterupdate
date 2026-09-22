using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.VendorFeedback.Commands.CreateVendorFeedback;

public class CreateVendorFeedbackResponse
{
    public VendorFeedbackDto? Feedback { get; set; }
}