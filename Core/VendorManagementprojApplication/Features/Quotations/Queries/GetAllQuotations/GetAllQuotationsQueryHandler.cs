using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Quotations.Queries.GetAllQuotations;

public class GetAllQuotationsQueryHandler : IRequestHandler<GetAllQuotationsQuery, GetAllQuotationsResponse>
{
    private readonly IQuotationRepository _quotationRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetAllQuotationsQueryHandler(
        IQuotationRepository quotationRepository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService)
    {
        _quotationRepository = quotationRepository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetAllQuotationsResponse> Handle(GetAllQuotationsQuery request, CancellationToken cancellationToken)
    {
        var quotations = await _quotationRepository.GetAllAsync();

        if (_currentUserService.IsOrganizationManager)
        {
            if (_currentUserService.OrganizationID.HasValue)
            {
                var orgOutlets = await _outletRepository.GetByOrganizationIdAsync(_currentUserService.OrganizationID.Value);
                var orgOutletIds = orgOutlets.Select(o => o.OutletID).ToHashSet();
                quotations = quotations.Where(q => q.Request != null && orgOutletIds.Contains(q.Request.OutletID)).ToList();
            }
        }
        else if (_currentUserService.IsOutletManager || _currentUserService.IsPurchaseManager)
        {
            if (_currentUserService.OutletID.HasValue)
            {
                quotations = quotations.Where(q => q.Request != null && q.Request.OutletID == _currentUserService.OutletID.Value).ToList();
            }
            else
            {
                quotations = new List<Quotation>();
            }
        }

        var list = quotations.Select(MapToDto).ToList();

        return new GetAllQuotationsResponse
        {
            Quotations = list
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
                TaxRate = item.TaxRate,
                TaxAmount = item.TaxAmount,
                TotalAmount = item.TotalAmount
            }).ToList()
        };
    }
}
