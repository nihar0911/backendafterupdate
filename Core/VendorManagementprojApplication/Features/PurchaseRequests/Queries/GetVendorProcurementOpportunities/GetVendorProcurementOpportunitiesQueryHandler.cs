using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Queries.GetVendorProcurementOpportunities;

public class GetVendorProcurementOpportunitiesQueryHandler
    : IRequestHandler<GetVendorProcurementOpportunitiesQuery, GetVendorProcurementOpportunitiesResponse>
{
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;
    private readonly IVendorProductRepository _vendorProductRepository;
    private readonly IQuotationRepository _quotationRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IUserRepository _userRepository;
    private readonly IVendorOpportunityResponseRepository _responseRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetVendorProcurementOpportunitiesQueryHandler(
        IPurchaseRequestRepository purchaseRequestRepository,
        IVendorProductRepository vendorProductRepository,
        IQuotationRepository quotationRepository,
        IOutletRepository outletRepository,
        IOrganizationRepository organizationRepository,
        IVendorRepository vendorRepository,
        IUserRepository userRepository,
        IVendorOpportunityResponseRepository responseRepository,
        ICurrentUserService currentUserService)
    {
        _purchaseRequestRepository = purchaseRequestRepository;
        _vendorProductRepository = vendorProductRepository;
        _quotationRepository = quotationRepository;
        _outletRepository = outletRepository;
        _organizationRepository = organizationRepository;
        _vendorRepository = vendorRepository;
        _userRepository = userRepository;
        _responseRepository = responseRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetVendorProcurementOpportunitiesResponse> Handle(
        GetVendorProcurementOpportunitiesQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsVendorManager || !_currentUserService.VendorID.HasValue)
        {
            throw new UnauthorizedAccessException(
                "Your vendor account is not configured or you do not have permission. Please contact an administrator.");
        }

        int vendorId = _currentUserService.VendorID.Value;

        var vendor = await _vendorRepository.GetByIdAsync(vendorId);
        string vendorName = vendor?.VendorName ?? $"Vendor #{vendorId}";

        var allVendorProducts = await _vendorProductRepository.GetAllAsync();
        var vendorProducts = allVendorProducts
            .Where(vp => vp.VendorID == vendorId && string.Equals(vp.Status, "Active", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(vp => vp.ProductID);

        if (!vendorProducts.Any())
        {
            return new GetVendorProcurementOpportunitiesResponse { Opportunities = new List<VendorProcurementOpportunityDto>() };
        }

        var allRequests = await _purchaseRequestRepository.GetAllAsync();
        var allQuotations = await _quotationRepository.GetAllAsync();
        var allOutlets = (await _outletRepository.GetAllAsync()).ToDictionary(o => o.OutletID);
        var allOrganizations = (await _organizationRepository.GetAllAsync()).ToDictionary(o => o.OrganizationID);
        var allUsers = (await _userRepository.GetAllAsync()).ToDictionary(u => u.UserID);
        
        var allResponses = await _responseRepository.GetAllAsync();
        var responsesByRequest = allResponses
            .GroupBy(r => r.RequestID)
            .ToDictionary(g => g.Key, g => g.ToList());

        var myResponses = allResponses
            .Where(r => r.VendorID == vendorId)
            .ToDictionary(r => $"{r.RequestID}_{r.ProductID}");

        var quotedRequestIds = allQuotations
            .Where(q => q.VendorID == vendorId)
            .Select(q => q.RequestID)
            .ToHashSet();

        var opportunities = new List<VendorProcurementOpportunityDto>();

        foreach (var req in allRequests)
        {
            if (quotedRequestIds.Contains(req.RequestID))
                continue;

            if (string.Equals(req.Status, "Cancelled", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(req.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                continue;

            allUsers.TryGetValue(req.CreatedByUserID, out var creatorUser);
            string creatorName = creatorUser?.Name ?? "Outlet Staff";
            string creatorRole = creatorUser?.Role?.RoleName ?? (creatorUser?.RoleID == 3 ? "Outlet Manager" : "Staff");

            // Check if this request has explicit selected vendor assignments
            bool hasExplicitResponses = responsesByRequest.TryGetValue(req.RequestID, out var reqResponses) &&
                                        reqResponses != null && reqResponses.Count > 0;

            foreach (var item in req.Items)
            {
                if (vendorProducts.TryGetValue(item.ProductID, out var vp))
                {
                    // Strict Selected Vendor Isolation:
                    // If the Purchase Request was dispatched to specific selected vendor(s),
                    // only those selected vendors are permitted to see and respond to the opportunity.
                    if (hasExplicitResponses)
                    {
                        bool isVendorSelected = reqResponses!.Any(r => r.VendorID == vendorId && r.ProductID == item.ProductID);
                        if (!isVendorSelected)
                        {
                            continue; // This vendor was NOT chosen by the user; do not expose this opportunity.
                        }
                    }

                    allOutlets.TryGetValue(req.OutletID, out var outlet);
                    Organization? org = null;
                    if (outlet != null)
                    {
                        allOrganizations.TryGetValue(outlet.OrganizationID, out org);
                    }

                    string outletDisplayName = outlet != null
                        ? (!string.IsNullOrWhiteSpace(outlet.OutletName) ? outlet.OutletName : $"{outlet.Address} Outlet")
                        : $"Outlet #{req.OutletID}";

                    string responseKey = $"{req.RequestID}_{item.ProductID}";
                    myResponses.TryGetValue(responseKey, out var resp);

                    string oppStatus = resp?.Status ?? "Pending";
                    string? rejectionReason = resp?.RejectionReason;
                    DateTime? responseDate = resp?.ResponseDate;

                    opportunities.Add(new VendorProcurementOpportunityDto
                    {
                        RequestID = req.RequestID,
                        RequestDate = req.RequestDate,
                        Status = req.Status,
                        OrganizationID = outlet?.OrganizationID ?? 0,
                        OrganizationName = org?.OrganizationName ?? "Organization",
                        OutletID = req.OutletID,
                        OutletName = outletDisplayName,
                        OutletAddress = outlet?.Address,
                        CreatedByUserID = req.CreatedByUserID,
                        CreatedByUserName = creatorName,
                        CreatedByUserRole = creatorRole,
                        ProductID = item.ProductID,
                        ProductName = item.Product?.ProductName ?? $"Product #{item.ProductID}",
                        Category = item.Product?.Category ?? "General",
                        Unit = item.Unit,
                        RequestedQuantity = item.Quantity,
                        VendorID = vendorId,
                        VendorName = vendorName,
                        UnitPrice = vp.UnitPrice,
                        EstimatedDeliveryDays = vp.EstimatedDeliveryDays,
                        OpportunityStatus = oppStatus,
                        RejectionReason = rejectionReason,
                        ResponseDate = responseDate,
                        HasQuotation = false,
                        QuotationID = null,
                        QuotationStatus = null
                    });
                }
            }
        }

        return new GetVendorProcurementOpportunitiesResponse
        {
            Opportunities = opportunities.OrderByDescending(o => o.RequestID).ToList()
        };
    }
}
