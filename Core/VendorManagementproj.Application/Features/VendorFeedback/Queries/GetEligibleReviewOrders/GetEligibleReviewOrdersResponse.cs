using System.Collections.Generic;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.VendorFeedback.Queries.GetEligibleReviewOrders;

public class GetEligibleReviewOrdersResponse
{
    public List<EligibleReviewOrderDto> Orders { get; set; } = new();
}
