using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Quotations.Queries.GetQuotationById;

public class GetQuotationByIdQueryHandler : IRequestHandler<GetQuotationByIdQuery, GetQuotationByIdResponse>
{
    private readonly IQuotationRepository _quotationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetQuotationByIdQueryHandler(
        IQuotationRepository quotationRepository,
        ICurrentUserService currentUserService)
    {
        _quotationRepository = quotationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetQuotationByIdResponse> Handle(GetQuotationByIdQuery request, CancellationToken cancellationToken)
    {
        var quotation = await _quotationRepository.GetByIdAsync(request.QuotationID);

        if (quotation == null)
            return new GetQuotationByIdResponse { Quotation = null };

        if (_currentUserService.IsPurchaseManager)
        {
            if (!_currentUserService.OutletID.HasValue || quotation.Request == null || quotation.Request.OutletID != _currentUserService.OutletID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view quotations belonging to another outlet.");
            }
        }

        return new GetQuotationByIdResponse
        {
            Quotation = MapToDto(quotation)
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
            Items = quotation.QuotationItems.Select(item => new QuotationItemDto
            {
                QuotationItemID = item.QuotationItemID,
                ProductID = item.ProductID,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                DiscountAmount = item.DiscountAmount,
                TaxRate = item.TaxRate,
                TaxAmount = item.TaxAmount,
                TotalAmount = item.TotalAmount
            }).ToList()
        };
    }
}
