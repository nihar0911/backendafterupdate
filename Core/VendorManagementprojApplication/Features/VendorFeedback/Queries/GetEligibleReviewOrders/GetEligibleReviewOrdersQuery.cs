using System.Collections.Generic;
using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.VendorFeedback.Queries.GetEligibleReviewOrders;

public class GetEligibleReviewOrdersQuery : IRequest<List<EligibleReviewOrderDto>>
{
}
