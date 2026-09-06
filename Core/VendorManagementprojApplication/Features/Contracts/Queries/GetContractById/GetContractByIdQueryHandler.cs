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

namespace VendorManagementprojApplication.Features.Contracts.Queries.GetContractById;

public class GetContractByIdQueryHandler : IRequestHandler<GetContractByIdQuery, GetContractByIdResponse>
{
    private readonly IContractRepository _contractRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly IQuotationRepository _quotationRepository;
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetContractByIdQueryHandler(
        IContractRepository contractRepository,
        IOutletRepository outletRepository,
        IQuotationRepository quotationRepository,
        IPurchaseRequestRepository purchaseRequestRepository,
        ICurrentUserService currentUserService)
    {
        _contractRepository = contractRepository;
        _outletRepository = outletRepository;
        _quotationRepository = quotationRepository;
        _purchaseRequestRepository = purchaseRequestRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetContractByIdResponse> Handle(GetContractByIdQuery request, CancellationToken cancellationToken)
    {
        var contract = await _contractRepository.GetByIdAsync(request.ContractID);

        if (contract == null)
        {
            return new GetContractByIdResponse { Contract = null };
        }

        if (_currentUserService.IsOrganizationManager)
        {
            if (!_currentUserService.OrganizationID.HasValue)
            {
                return new GetContractByIdResponse { Contract = null };
            }
            var targetOutlet = contract.Outlet ?? await _outletRepository.GetByIdAsync(contract.OutletID);
            if (targetOutlet == null || targetOutlet.OrganizationID != _currentUserService.OrganizationID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view contracts outside your organization.");
            }
        }
        else if (_currentUserService.IsOutletManager)
        {
            if (!_currentUserService.OutletID.HasValue || contract.OutletID != _currentUserService.OutletID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view contracts belonging to another outlet.");
            }
        }

        Quotation? matchingQuotation = null;
        if (contract.QuotationID.HasValue)
        {
            matchingQuotation = await _quotationRepository.GetByIdAsync(contract.QuotationID.Value);
        }

        if (matchingQuotation == null)
        {
            var allQuotations = await _quotationRepository.GetAllAsync();
            var firstAlloc = contract.VendorAllocations?.FirstOrDefault();
            int vendorId = firstAlloc?.VendorID ?? 0;

            matchingQuotation = allQuotations
                .Where(q => q.VendorID == vendorId &&
                            string.Equals(q.Status, "Accepted", StringComparison.OrdinalIgnoreCase) &&
                            q.QuotationItems.Any(qi => qi.ProductID == contract.ProductID && qi.Quantity == contract.TotalQuantity))
                .OrderByDescending(q => q.QuotationID)
                .FirstOrDefault();
        }

        PurchaseRequest? matchingPr = null;
        if (matchingQuotation != null)
        {
            matchingPr = await _purchaseRequestRepository.GetByIdAsync(matchingQuotation.RequestID);
        }

        var dto = MapToDto(contract, matchingQuotation, matchingPr);

        return new GetContractByIdResponse
        {
            Contract = dto
        };
    }

    private static ContractDto MapToDto(Contract contract, Quotation? quotation, PurchaseRequest? purchaseRequest)
    {
        var firstAlloc = contract.VendorAllocations?.FirstOrDefault();
        var qItem = quotation?.QuotationItems?.FirstOrDefault(qi => qi.ProductID == contract.ProductID) ?? quotation?.QuotationItems?.FirstOrDefault();

        decimal unitPrice = qItem?.UnitPrice ?? 0;
        decimal taxAmount = qItem?.TaxAmount ?? 0;
        decimal totalAmount = qItem?.TotalAmount > 0 ? qItem.TotalAmount : (unitPrice * contract.TotalQuantity + taxAmount);

        return new ContractDto
        {
            ContractID = contract.ContractID,
            QuotationID = contract.QuotationID ?? quotation?.QuotationID,
            OutletID = contract.OutletID,
            OutletName = contract.Outlet?.OutletName ?? $"Outlet #{contract.OutletID}",
            OrganizationID = contract.Outlet?.OrganizationID ?? 0,
            OrganizationName = contract.Outlet?.Organization?.OrganizationName ?? "Organization",
            ProductID = contract.ProductID,
            ProductName = contract.Product?.ProductName ?? $"Product #{contract.ProductID}",
            Unit = contract.Product?.Unit ?? "Kg",
            RequestID = purchaseRequest?.RequestID ?? quotation?.RequestID,
            VendorID = firstAlloc?.VendorID ?? quotation?.VendorID,
            VendorName = firstAlloc?.Vendor?.VendorName ?? "Vendor",
            TotalQuantity = contract.TotalQuantity,
            UsedQuantity = contract.UsedQuantity,
            UnitPrice = unitPrice,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            PaymentMethod = contract.PaymentMethod,
            Status = contract.Status,
            Allocations = contract.VendorAllocations?.Select(a => new ContractVendorAllocationDto
            {
                ContractVendorAllocationID = a.ContractVendorAllocationID,
                VendorID = a.VendorID,
                VendorName = a.Vendor?.VendorName ?? $"Vendor #{a.VendorID}",
                AllocationPercentage = a.AllocationPercentage,
                AllocatedQuantity = a.AllocatedQuantity,
                UsedQuantity = a.UsedQuantity,
                Status = a.Status
            }).ToList() ?? new List<ContractVendorAllocationDto>()
        };
    }
}
