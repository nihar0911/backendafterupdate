using System.Collections.Generic;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.VendorFeedback.Queries.GetEligibleReviewOrders;

public class GetEligibleReviewOrdersResponse
{
    public List<EligibleReviewOrderDto> Orders { get; set; } = new();
}
