using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.VendorFeedback.Queries.GetEligibleReviewOrders;

public class GetEligibleReviewOrdersQueryHandler : IRequestHandler<GetEligibleReviewOrdersQuery, List<EligibleReviewOrderDto>>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IVendorFeedbackRepository _feedbackRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetEligibleReviewOrdersQueryHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IVendorFeedbackRepository feedbackRepository,
        ICurrentUserService currentUserService)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _feedbackRepository = feedbackRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<EligibleReviewOrderDto>> Handle(
        GetEligibleReviewOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var allPOs = await _purchaseOrderRepository.GetAllAsync();
        var deliveredPOs = allPOs.Where(po => string.Equals(po.Status, "Delivered", StringComparison.OrdinalIgnoreCase)).ToList();

        if (_currentUserService.IsOrganizationManager)
        {
            if (_currentUserService.OrganizationID.HasValue)
            {
                deliveredPOs = deliveredPOs.Where(po => po.Outlet != null && po.Outlet.OrganizationID == _currentUserService.OrganizationID.Value).ToList();
            }
            else
            {
                return new List<EligibleReviewOrderDto>();
            }
        }
        else if (_currentUserService.IsOutletManager || _currentUserService.IsPurchaseManager)
        {
            if (_currentUserService.OutletID.HasValue)
            {
                deliveredPOs = deliveredPOs.Where(po => po.OutletID == _currentUserService.OutletID.Value).ToList();
            }
            else
            {
                return new List<EligibleReviewOrderDto>();
            }
        }

        var allFeedback = await _feedbackRepository.GetAllAsync();
        var reviewedKeys = allFeedback.Select(f => (f.PurchaseOrderID, f.POItemID)).ToHashSet();

        var result = new List<EligibleReviewOrderDto>();
        foreach (var po in deliveredPOs)
        {
            if (po.Items == null) continue;
            foreach (var item in po.Items)
            {
                bool isReviewed = reviewedKeys.Contains((po.PurchaseOrderID, item.POItemID));
                result.Add(new EligibleReviewOrderDto
                {
                    PurchaseOrderID = po.PurchaseOrderID,
                    POItemID = item.POItemID,
                    VendorID = po.VendorID,
                    VendorName = po.Vendor?.VendorName ?? string.Empty,
                    OutletID = po.OutletID,
                    OutletName = po.Outlet?.OutletName ?? string.Empty,
                    ProductID = item.ProductID,
                    ProductName = item.Product?.ProductName ?? string.Empty,
                    Quantity = item.Quantity,
                    Unit = item.Product?.Unit ?? "Units",
                    ActualDeliveryDate = po.ActualDeliveryDate,
                    ExpectedDeliveryDate = po.ExpectedDeliveryDate,
                    AlreadyReviewed = isReviewed
                });
            }
        }

        return result.OrderByDescending(r => r.ActualDeliveryDate ?? DateTime.MinValue).ToList();
    }
}
