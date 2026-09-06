using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorFeedbackEntity = VendorManagementprojDomain.Entities.VendorFeedback;

namespace VendorManagementprojApplication.Features.VendorFeedback.Queries.GetAllVendorFeedback;

public class GetAllVendorFeedbackQueryHandler
    : IRequestHandler<GetAllVendorFeedbackQuery, GetAllVendorFeedbackResponse>
{
    private readonly IVendorFeedbackRepository _feedbackRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetAllVendorFeedbackQueryHandler(
        IVendorFeedbackRepository feedbackRepository,
        ICurrentUserService currentUserService)
    {
        _feedbackRepository = feedbackRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetAllVendorFeedbackResponse> Handle(
        GetAllVendorFeedbackQuery request,
        CancellationToken cancellationToken)
    {
        var feedback = await _feedbackRepository.GetAllAsync();

        if (_currentUserService.IsOrganizationManager)
        {
            if (_currentUserService.OrganizationID.HasValue)
            {
                feedback = feedback.Where(f => f.Outlet != null && f.Outlet.OrganizationID == _currentUserService.OrganizationID.Value).ToList();
            }
            else
            {
                feedback = new List<VendorFeedbackEntity>();
            }
        }
        else if (_currentUserService.IsOutletManager || _currentUserService.IsPurchaseManager)
        {
            if (_currentUserService.OutletID.HasValue)
            {
                feedback = feedback.Where(f => f.OutletID == _currentUserService.OutletID.Value).ToList();
            }
            else
            {
                feedback = new List<VendorFeedbackEntity>();
            }
        }
        else if (string.Equals(_currentUserService.Role, "Vendor Manager", StringComparison.OrdinalIgnoreCase))
        {
            if (_currentUserService.VendorID.HasValue)
            {
                feedback = feedback.Where(f => f.VendorID == _currentUserService.VendorID.Value).ToList();
            }
            else
            {
                feedback = new List<VendorFeedbackEntity>();
            }
        }

        return new GetAllVendorFeedbackResponse
        {
            Feedback = feedback.Select(f => new VendorFeedbackDto
            {
                FeedbackID = f.FeedbackID,
                VendorID = f.VendorID,
                VendorName = f.Vendor?.VendorName ?? string.Empty,
                OutletID = f.OutletID,
                OutletName = f.Outlet?.OutletName ?? string.Empty,
                PurchaseOrderID = f.PurchaseOrderID,
                POItemID = f.POItemID,
                ProductName = f.POItem?.Product?.ProductName ?? string.Empty,
                RatedByUserID = f.RatedByUserID,
                RatedByUserName = f.RatedByUser?.Name ?? string.Empty,
                Rating = f.Rating,
                ProductQualityRating = f.ProductQualityRating,
                DeliveryRating = f.DeliveryRating,
                Review = f.Review,
                FeedbackDate = f.FeedbackDate
            }).ToList()
        };
    }
}
