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

namespace VendorManagementprojApplication.Features.Quotations.Queries.GetVendorQuotations;

public class GetVendorQuotationsQueryHandler
    : IRequestHandler<GetVendorQuotationsQuery, GetVendorQuotationsResponse>
{
    private readonly IQuotationRepository _quotationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetVendorQuotationsQueryHandler(
        IQuotationRepository quotationRepository,
        ICurrentUserService currentUserService)
    {
        _quotationRepository = quotationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetVendorQuotationsResponse> Handle(
        GetVendorQuotationsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsVendorManager || !_currentUserService.VendorID.HasValue)
        {
            throw new UnauthorizedAccessException(
                "Your vendor account is not configured or you do not have permission. Please contact an administrator.");
        }

        int vendorId = _currentUserService.VendorID.Value;

        var vendorQuotations = (await _quotationRepository.GetByVendorIdAsync(vendorId))
            .OrderByDescending(q => q.QuotationID)
            .Select(MapToDto)
            .ToList();

        return new GetVendorQuotationsResponse
        {
            Quotations = vendorQuotations
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
            Items = quotation.QuotationItems?.Select(item => new QuotationItemDto
            {
                QuotationItemID = item.QuotationItemID,
                ProductID = item.ProductID,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TaxRate = item.TaxRate,
                TaxAmount = item.TaxAmount,
                TotalAmount = item.TotalAmount
            }).ToList() ?? new List<QuotationItemDto>()
        };
    }
}
