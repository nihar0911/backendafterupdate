using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.VendorFeedback.Queries.GetVendorFeedbackById;

public class GetVendorFeedbackByIdQueryHandler
    : IRequestHandler<GetVendorFeedbackByIdQuery, GetVendorFeedbackByIdResponse>
{
    private readonly IVendorFeedbackRepository _feedbackRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetVendorFeedbackByIdQueryHandler(
        IVendorFeedbackRepository feedbackRepository,
        ICurrentUserService currentUserService)
    {
        _feedbackRepository = feedbackRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetVendorFeedbackByIdResponse> Handle(
        GetVendorFeedbackByIdQuery request,
        CancellationToken cancellationToken)
    {
        var feedback = await _feedbackRepository.GetByIdAsync(request.FeedbackID);

        if (feedback == null)
        {
            return new GetVendorFeedbackByIdResponse
            {
                Feedback = null
            };
        }

        // Security / Role Authorization Check
        if ((_currentUserService.IsPurchaseManager || _currentUserService.IsOutletManager) && _currentUserService.OutletID.HasValue)
        {
            if (feedback.OutletID != _currentUserService.OutletID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view vendor feedback belonging to another outlet.");
            }
        }
        else if ((_currentUserService.IsPurchaseManager || _currentUserService.IsOutletManager) && !_currentUserService.OutletID.HasValue)
        {
            throw new UnauthorizedAccessException("You are not authorized to view vendor feedback without an assigned outlet.");
        }
        else if (_currentUserService.IsVendorManager && _currentUserService.VendorID.HasValue)
        {
            if (feedback.VendorID != _currentUserService.VendorID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view feedback belonging to another vendor.");
            }
        }
        else if (_currentUserService.IsOrganizationManager && _currentUserService.OrganizationID.HasValue)
        {
            if (feedback.Outlet != null && feedback.Outlet.OrganizationID != _currentUserService.OrganizationID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view vendor feedback outside your organization.");
            }
        }

        return new GetVendorFeedbackByIdResponse
        {
            Feedback = new VendorFeedbackDto
            {
                FeedbackID = feedback.FeedbackID,
                VendorID = feedback.VendorID,
                VendorName = feedback.Vendor?.VendorName ?? string.Empty,
                OutletID = feedback.OutletID,
                OutletName = !string.IsNullOrWhiteSpace(feedback.Outlet?.OutletName) ? feedback.Outlet.OutletName : (!string.IsNullOrWhiteSpace(feedback.Outlet?.Address) ? feedback.Outlet.Address : string.Empty),
                PurchaseOrderID = feedback.PurchaseOrderID,
                POItemID = feedback.POItemID,
                ProductID = feedback.POItem?.ProductID ?? 0,
                ProductName = feedback.POItem?.Product?.ProductName ?? string.Empty,
                RatedByUserID = feedback.RatedByUserID,
                RatedByUserName = feedback.RatedByUser?.Name ?? string.Empty,
                Rating = feedback.Rating,
                ProductQualityRating = feedback.ProductQualityRating,
                DeliveryRating = feedback.DeliveryRating,
                Review = feedback.Review,
                FeedbackDate = feedback.FeedbackDate
            }
        };
    }
}

