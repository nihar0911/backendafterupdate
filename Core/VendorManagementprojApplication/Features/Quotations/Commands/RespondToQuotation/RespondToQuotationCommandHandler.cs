using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Quotations.Commands.RespondToQuotation;

public class RespondToQuotationCommandHandler
    : IRequestHandler<RespondToQuotationCommand, RespondToQuotationResponse>
{
    private readonly IQuotationRepository _quotationRepository;
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;

    public RespondToQuotationCommandHandler(
        IQuotationRepository quotationRepository,
        IPurchaseRequestRepository purchaseRequestRepository)
    {
        _quotationRepository = quotationRepository;
        _purchaseRequestRepository = purchaseRequestRepository;
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
            var purchaseRequest = await _purchaseRequestRepository.GetByIdAsync(quotation.RequestID);
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
                    DiscountAmount = item.DiscountAmount,
                    TaxRate = item.TaxRate,
                    TaxAmount = item.TaxAmount,
                    TotalAmount = item.TotalAmount
                })
                .ToList()
        };
    }
}