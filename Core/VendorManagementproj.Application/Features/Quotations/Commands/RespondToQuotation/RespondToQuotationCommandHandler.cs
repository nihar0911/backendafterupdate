using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Quotations.Commands.RespondToQuotation;

public class RespondToQuotationCommandHandler
    : IRequestHandler<RespondToQuotationCommand, RespondToQuotationResponse>
{
    private readonly IQuotationRepository _quotationRepository;
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;
    private readonly ICurrentUserService _currentUserService;

    public RespondToQuotationCommandHandler(
        IQuotationRepository quotationRepository,
        IPurchaseRequestRepository purchaseRequestRepository,
        ICurrentUserService currentUserService)
    {
        _quotationRepository = quotationRepository;
        _purchaseRequestRepository = purchaseRequestRepository;
        _currentUserService = currentUserService;
    }

    public async Task<RespondToQuotationResponse> Handle(
        RespondToQuotationCommand request,
        CancellationToken cancellationToken)
    {
        var quotation =
            await _quotationRepository.GetByIdAsync(
                request.QuotationID);

        if (quotation == null)
            throw new InvalidOperationException(
                "Quotation does not exist.");

        var purchaseRequest = await _purchaseRequestRepository.GetByIdAsync(quotation.RequestID);

        bool isAuthorized = false;
        if (_currentUserService.IsAdmin)
        {
            isAuthorized = true;
        }
        else if (_currentUserService.IsPurchaseManager && _currentUserService.OutletID.HasValue)
        {
            if (purchaseRequest != null && purchaseRequest.OutletID == _currentUserService.OutletID.Value)
            {
                isAuthorized = true;
            }
        }

        if (!isAuthorized)
        {
            throw new UnauthorizedAccessException("You are not authorized to respond to this quotation.");
        }

        if (!string.Equals(
                request.Status,
                "Accepted",
                StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(
                request.Status,
                "Rejected",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Status must be either Accepted or Rejected.");
        }

        if (!string.Equals(
                quotation.Status,
                "Pending",
                StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(
                quotation.Status,
                "Submitted",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "This quotation has already been responded to.");
        }

        if (string.Equals(
                request.Status,
                "Accepted",
                StringComparison.OrdinalIgnoreCase) &&
            quotation.ValidUntil < DateTime.Now)
        {
            throw new InvalidOperationException(
                "This quotation has expired and cannot be accepted.");
        }

        quotation.Status = request.Status;

        if (string.Equals(
                request.Status,
                "Accepted",
                StringComparison.OrdinalIgnoreCase))
        {
            if (purchaseRequest != null)
            {
                purchaseRequest.Status = "Approved";
                await _purchaseRequestRepository.UpdateAsync(purchaseRequest.RequestID, purchaseRequest);
            }
        }

        var updatedQuotation =
            await _quotationRepository.UpdateAsync(
                request.QuotationID,
                quotation);

        if (updatedQuotation == null)
            throw new InvalidOperationException(
                "Unable to update quotation.");

        return new RespondToQuotationResponse
        {
            Quotation = MapToDto(updatedQuotation)
        };
    }

    private static QuotationDto MapToDto(
        VendorManagementprojDomain.Entities.Quotation quotation)
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
                    TaxRate = item.TaxRate,
                    TaxAmount = item.TaxAmount,
                    TotalAmount = item.TotalAmount
                })
                .ToList()
        };
    }
}