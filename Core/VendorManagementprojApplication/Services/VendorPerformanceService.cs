using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Services;

public class VendorPerformanceService : IVendorPerformanceService
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IDeliveryRecordRepository _deliveryRecordRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IVendorFeedbackRepository _feedbackRepository;

    public VendorPerformanceService(
        IVendorRepository vendorRepository,
        IPurchaseOrderRepository purchaseOrderRepository,
        IDeliveryRecordRepository deliveryRecordRepository,
        IInvoiceRepository invoiceRepository,
        IVendorFeedbackRepository feedbackRepository)
    {
        _vendorRepository = vendorRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _deliveryRecordRepository = deliveryRecordRepository;
        _invoiceRepository = invoiceRepository;
        _feedbackRepository = feedbackRepository;
    }

    public async Task<List<VendorPerformanceSummaryDto>> GetOrganizationVendorsPerformanceAsync(int? organizationId = null)
    {
        var vendors = await _vendorRepository.GetAllAsync();
        var activeVendors = vendors.Where(v => string.Equals(v.Status, "Active", StringComparison.OrdinalIgnoreCase)).ToList();

        var allPOs = await _purchaseOrderRepository.GetAllAsync();
        var allDeliveries = await _deliveryRecordRepository.GetAllAsync();
        var allInvoices = await _invoiceRepository.GetAllAsync();
        var allFeedback = await _feedbackRepository.GetAllAsync();

        // Scope by organization if specified
        if (organizationId.HasValue && organizationId.Value > 0)
        {
            int orgId = organizationId.Value;
            allPOs = allPOs.Where(po => po.Outlet != null && po.Outlet.OrganizationID == orgId).ToList();
            allDeliveries = allDeliveries.Where(dr => dr.PurchaseOrder != null && dr.PurchaseOrder.Outlet != null && dr.PurchaseOrder.Outlet.OrganizationID == orgId).ToList();
            allInvoices = allInvoices.Where(inv => inv.Outlet != null && inv.Outlet.OrganizationID == orgId).ToList();
            // Feedback is platform-wide reputation across all verified buyers
        }

        var result = new List<VendorPerformanceSummaryDto>();

        foreach (var vendor in activeVendors)
        {
            var summary = CalculateSummary(
                vendor.VendorID,
                vendor.VendorName,
                allPOs.Where(po => po.VendorID == vendor.VendorID).ToList(),
                allDeliveries.Where(dr => dr.PurchaseOrder != null && dr.PurchaseOrder.VendorID == vendor.VendorID).ToList(),
                allInvoices.Where(inv => inv.VendorID == vendor.VendorID).ToList(),
                allFeedback.Where(f => f.VendorID == vendor.VendorID).ToList());

            result.Add(summary);
        }

        return result
            .OrderByDescending(s => s.HasDeliveryHistory)
            .ThenByDescending(s => s.CompletedDeliveries)
            .ThenByDescending(s => s.OnTimeDeliveryRate ?? -1)
            .ThenBy(s => s.VendorName)
            .ToList();
    }

    public async Task<VendorPerformanceSummaryDto?> GetVendorPerformanceAsync(int vendorId, int? organizationId = null)
    {
        var vendor = await _vendorRepository.GetByIdAsync(vendorId);
        if (vendor == null) return null;

        var allPOs = await _purchaseOrderRepository.GetByVendorIdAsync(vendorId);
        var allDeliveries = await _deliveryRecordRepository.GetByVendorIdAsync(vendorId);
        var allInvoices = await _invoiceRepository.GetByVendorIdAsync(vendorId);
        var allFeedback = await _feedbackRepository.GetByVendorIdAsync(vendorId, organizationId);

        if (organizationId.HasValue && organizationId.Value > 0)
        {
            int orgId = organizationId.Value;
            allPOs = allPOs.Where(po => po.Outlet != null && po.Outlet.OrganizationID == orgId).ToList();
            allDeliveries = allDeliveries.Where(dr => dr.PurchaseOrder != null && dr.PurchaseOrder.Outlet != null && dr.PurchaseOrder.Outlet.OrganizationID == orgId).ToList();
            allInvoices = allInvoices.Where(inv => inv.Outlet != null && inv.Outlet.OrganizationID == orgId).ToList();
            // Feedback is platform-wide reputation across all verified buyers
        }

        return CalculateSummary(
            vendor.VendorID,
            vendor.VendorName,
            allPOs,
            allDeliveries,
            allInvoices,
            allFeedback);
    }

    private static VendorPerformanceSummaryDto CalculateSummary(
        int vendorId,
        string vendorName,
        List<PurchaseOrder> vendorPOs,
        List<DeliveryRecord> vendorDeliveries,
        List<Invoice> vendorInvoices,
        List<VendorFeedback> vendorFeedback)
    {
        var summary = new VendorPerformanceSummaryDto
        {
            VendorID = vendorId,
            VendorName = vendorName,
            TotalPurchaseOrders = vendorPOs.Count
        };

        // 1. Delivery Performance
        var completedPOs = vendorPOs
            .Where(po => string.Equals(po.Status, "Delivered", StringComparison.OrdinalIgnoreCase))
            .ToList();

        summary.CompletedDeliveries = completedPOs.Count;

        if (completedPOs.Count > 0)
        {
            int onTimeCount = 0;
            int delayedCount = 0;
            double totalDelayDays = 0;

            foreach (var po in completedPOs)
            {
                DateTime? actualDate = po.ActualDeliveryDate;
                DateTime? expectedDate = po.ExpectedDeliveryDate;

                // Fallback to delivery record date if ActualDeliveryDate on PO is null
                if (!actualDate.HasValue)
                {
                    var confirmedDR = vendorDeliveries.FirstOrDefault(d => d.PurchaseOrderID == po.PurchaseOrderID && string.Equals(d.Status, "Confirmed", StringComparison.OrdinalIgnoreCase));
                    if (confirmedDR != null)
                    {
                        actualDate = confirmedDR.ConfirmedAt ?? confirmedDR.DeliveryDate;
                    }
                }

                if (actualDate.HasValue && expectedDate.HasValue)
                {
                    if (actualDate.Value.Date <= expectedDate.Value.Date)
                    {
                        onTimeCount++;
                    }
                    else
                    {
                        delayedCount++;
                        totalDelayDays += (actualDate.Value.Date - expectedDate.Value.Date).TotalDays;
                    }
                }
                else
                {
                    // If no explicit actual date recorded but status is Delivered, consider on-time
                    onTimeCount++;
                }
            }

            summary.OnTimeDeliveries = onTimeCount;
            summary.DelayedDeliveries = delayedCount;
            summary.OnTimeDeliveryRate = Math.Round(((decimal)onTimeCount / completedPOs.Count) * 100m, 1);
            summary.AverageDelayDays = delayedCount > 0 ? Math.Round((decimal)(totalDelayDays / delayedCount), 1) : 0m;
        }
        else
        {
            // Cold start - NO delivery history
            summary.OnTimeDeliveryRate = null;
            summary.AverageDelayDays = null;
        }

        // 2. Quality / Spoilage Performance
        var confirmedDeliveries = vendorDeliveries
            .Where(d => string.Equals(d.Status, "Confirmed", StringComparison.OrdinalIgnoreCase))
            .ToList();

        decimal totalReceived = confirmedDeliveries.Sum(d => d.ReceivedQuantity);
        decimal totalSpoiled = confirmedDeliveries.Sum(d => d.SpoiledQuantity);

        summary.TotalReceivedQuantity = totalReceived;
        summary.TotalSpoiledQuantity = totalSpoiled;
        summary.NetAcceptedQuantity = totalReceived - totalSpoiled;

        if (totalReceived > 0)
        {
            summary.SpoilageRate = Math.Round((totalSpoiled / totalReceived) * 100m, 2);
        }
        else
        {
            summary.SpoilageRate = null;
        }

        // 3. Invoice Performance
        summary.TotalInvoices = vendorInvoices.Count;
        int approvedCount = vendorInvoices.Count(i => string.Equals(i.Status, "Approved", StringComparison.OrdinalIgnoreCase) || string.Equals(i.Status, "Paid", StringComparison.OrdinalIgnoreCase));
        int rejectedCount = vendorInvoices.Count(i => string.Equals(i.Status, "Rejected", StringComparison.OrdinalIgnoreCase));

        summary.ApprovedInvoices = approvedCount;
        summary.RejectedInvoices = rejectedCount;

        if (vendorInvoices.Count > 0)
        {
            summary.InvoiceApprovalRate = Math.Round(((decimal)approvedCount / vendorInvoices.Count) * 100m, 1);
        }
        else
        {
            summary.InvoiceApprovalRate = null;
        }

        // 4. Reviews & Feedback
        summary.TotalReviews = vendorFeedback.Count;

        if (vendorFeedback.Count > 0)
        {
            summary.AverageRating = Math.Round(vendorFeedback.Average(f => f.Rating), 1);
            summary.AverageQualityRating = Math.Round(vendorFeedback.Average(f => f.ProductQualityRating), 1);
            summary.AverageDeliveryRating = Math.Round(vendorFeedback.Average(f => f.DeliveryRating), 1);
        }
        else
        {
            summary.AverageRating = null;
            summary.AverageQualityRating = null;
            summary.AverageDeliveryRating = null;
        }

        return summary;
    }
}
