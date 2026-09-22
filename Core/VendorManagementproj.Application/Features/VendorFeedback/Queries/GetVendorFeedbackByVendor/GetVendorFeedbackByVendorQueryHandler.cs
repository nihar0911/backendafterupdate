using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.VendorFeedback.Queries.GetVendorFeedbackByVendor;

public class GetVendorFeedbackByVendorQueryHandler
    : IRequestHandler<GetVendorFeedbackByVendorQuery, GetVendorFeedbackByVendorResponse>
{
    private readonly IVendorFeedbackRepository _feedbackRepository;

    public GetVendorFeedbackByVendorQueryHandler(IVendorFeedbackRepository feedbackRepository)
    {
        _feedbackRepository = feedbackRepository;
    }

    public async Task<GetVendorFeedbackByVendorResponse> Handle(
        GetVendorFeedbackByVendorQuery request,
        CancellationToken cancellationToken)
    {
        var list = await _feedbackRepository.GetByVendorIdAsync(request.VendorID, null, request.ProductID);
        var dtos = list.Select(f => new VendorFeedbackDto
        {
            FeedbackID = f.FeedbackID,
            VendorID = f.VendorID,
            VendorName = f.Vendor?.VendorName ?? string.Empty,
            OutletID = f.OutletID,
            OutletName = f.Outlet?.OutletName ?? string.Empty,
            PurchaseOrderID = f.PurchaseOrderID,
            POItemID = f.POItemID,
            ProductID = f.POItem?.ProductID ?? 0,
            ProductName = f.POItem?.Product?.ProductName ?? string.Empty,
            RatedByUserID = f.RatedByUserID,
            RatedByUserName = f.RatedByUser?.Name ?? string.Empty,
            Rating = f.Rating,
            ProductQualityRating = f.ProductQualityRating,
            DeliveryRating = f.DeliveryRating,
            Review = f.Review,
            FeedbackDate = f.FeedbackDate
        }).ToList();

        return new GetVendorFeedbackByVendorResponse
        {
            Feedback = dtos
        };
    }
}
