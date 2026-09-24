using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.Contracts.Services;
using VendorManagementproj.Application.DTOs;
using VendorManagementproj.Domain.Entities;

namespace VendorManagementproj.Application.Features.DeliveryRecords.Queries.GetMyDeliveries;

public class GetMyDeliveriesQueryHandler : IRequestHandler<GetMyDeliveriesQuery, GetMyDeliveriesResponse>
{
    private readonly IDeliveryRecordRepository _deliveryRecordRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMyDeliveriesQueryHandler(
        IDeliveryRecordRepository deliveryRecordRepository,
        ICurrentUserService currentUserService)
    {
        _deliveryRecordRepository = deliveryRecordRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetMyDeliveriesResponse> Handle(
        GetMyDeliveriesQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsVendorManager || !_currentUserService.VendorID.HasValue)
        {
            throw new UnauthorizedAccessException(
                "Your vendor account is not configured or you do not have permission. Please contact an administrator.");
        }

        int vendorId = _currentUserService.VendorID.Value;

        var confirmedDeliveries = await _deliveryRecordRepository.GetConfirmedByVendorAsync(vendorId);

        var deliveryDtos = confirmedDeliveries
            .OrderByDescending(d => d.DeliveryDate)
            .ThenByDescending(d => d.DeliveryRecordID)
            .Select(d => new VendorDeliveryItemDto
            {
                DeliveryRecordID = d.DeliveryRecordID,
                PurchaseOrderID = d.PurchaseOrderID,
                PONumber = $"PO-#{d.PurchaseOrderID}",
                POItemID = d.POItemID,
                ProductID = d.PurchaseOrderItem?.ProductID ?? 0,
                ProductName = d.PurchaseOrderItem?.Product?.ProductName ?? "Unknown Product",
                OutletID = d.PurchaseOrder?.OutletID ?? 0,
                OutletName = d.PurchaseOrder?.Outlet?.OutletName ?? "Unknown Outlet",
                OrderedQuantity = d.OrderedQuantity,
                ReceivedQuantity = d.ReceivedQuantity,
                SpoiledQuantity = d.SpoiledQuantity,
                SpoilagePercentage = d.SpoilagePercentage,
                Unit = d.PurchaseOrderItem?.Product?.Unit ?? string.Empty,
                DeliveryDate = d.DeliveryDate,
                DeliveryStatus = !string.IsNullOrWhiteSpace(d.PurchaseOrder?.DeliveryStatus)
                    ? d.PurchaseOrder.DeliveryStatus
                    : (!string.IsNullOrWhiteSpace(d.PurchaseOrder?.Status) && string.Equals(d.PurchaseOrder.Status, "Delivered", StringComparison.OrdinalIgnoreCase)
                        ? "Delivered"
                        : d.Status),
                Status = d.Status,
                ConfirmedAt = d.ConfirmedAt,
                ConfirmedByUserName = d.ConfirmedByUser?.Name
            })
            .ToList();

        return new GetMyDeliveriesResponse
        {
            Deliveries = deliveryDtos,
            TotalDeliveries = deliveryDtos.Count,
            TotalReceivedQuantity = deliveryDtos.Sum(d => d.ReceivedQuantity),
            TotalSpoiledQuantity = deliveryDtos.Sum(d => d.SpoiledQuantity),
            DistinctProductsCount = deliveryDtos.Select(d => d.ProductID).Where(id => id > 0).Distinct().Count()
        };
    }
}
