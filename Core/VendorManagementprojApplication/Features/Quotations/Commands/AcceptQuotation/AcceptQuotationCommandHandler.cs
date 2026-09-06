using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Quotations.Commands.AcceptQuotation;

public class AcceptQuotationCommandHandler : IRequestHandler<AcceptQuotationCommand, AcceptQuotationResponse>
{
    private readonly IQuotationRepository _quotationRepository;
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public AcceptQuotationCommandHandler(
        IQuotationRepository quotationRepository,
        IPurchaseRequestRepository purchaseRequestRepository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService)
    {
        _quotationRepository = quotationRepository;
        _purchaseRequestRepository = purchaseRequestRepository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<AcceptQuotationResponse> Handle(AcceptQuotationCommand request, CancellationToken cancellationToken)
    {
        var quotation = await _quotationRepository.GetByIdAsync(request.QuotationID);
        if (quotation == null)
            throw new InvalidOperationException("Quotation does not exist.");

        if (!string.Equals(quotation.Status, "Pending", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(quotation.Status, "Submitted", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("This quotation has already been processed.");
        }

        if (quotation.ValidUntil < DateTime.Now)
        {
            throw new InvalidOperationException("This quotation has expired and cannot be accepted.");
        }

        var purchaseRequest = await _purchaseRequestRepository.GetByIdAsync(quotation.RequestID);
        if (_currentUserService.IsPurchaseManager)
        {
            if (!_currentUserService.OutletID.HasValue || purchaseRequest == null || purchaseRequest.OutletID != _currentUserService.OutletID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to accept quotations belonging to another outlet.");
            }
        }
        else if (purchaseRequest != null && _currentUserService.IsOrganizationManager && _currentUserService.OrganizationID.HasValue)
        {
            var outlet = await _outletRepository.GetByIdAsync(purchaseRequest.OutletID);
            if (outlet != null && outlet.OrganizationID != _currentUserService.OrganizationID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to accept quotations outside your organization.");
            }
        }

        if (purchaseRequest != null)
        {
            purchaseRequest.Status = "Approved";
            await _purchaseRequestRepository.UpdateAsync(purchaseRequest.RequestID, purchaseRequest);
        }

        quotation.Status = "Accepted";
        var updatedQuotation = await _quotationRepository.UpdateAsync(request.QuotationID, quotation);
        if (updatedQuotation == null)
            throw new InvalidOperationException("Unable to update quotation status.");

        return new AcceptQuotationResponse
        {
            Quotation = MapToDto(updatedQuotation)
        };
    }

    private static QuotationDto MapToDto(Quotation quotation)
    {
        return new QuotationDto
        {
            QuotationID = quotation.QuotationID,
            RequestID = quotation.RequestID,
            VendorID = quotation.VendorID,
            ValidUntil = quotation.ValidUntil,
            Status = quotation.Status,
            Items = quotation.QuotationItems
                .Select(item => new QuotationItemDto
                {
                    QuotationItemID = item.QuotationItemID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    DiscountAmount = item.DiscountAmount,
                    TaxRate = item.TaxRate,
                    TaxAmount = item.TaxAmount,
                    TotalAmount = item.TotalAmount
                })
                .ToList()
        };
    }
}
