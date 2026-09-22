using System.Collections.Generic;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.VendorFeedback.Queries.GetVendorFeedbackByVendor;

public class GetVendorFeedbackByVendorResponse
{
    public List<VendorFeedbackDto> Feedback { get; set; } = new();
}
