using System.Collections.Generic;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.VendorFeedback.Queries.GetVendorFeedbackByVendor;

public class GetVendorFeedbackByVendorResponse
{
    public List<VendorFeedbackDto> Feedback { get; set; } = new();
}
