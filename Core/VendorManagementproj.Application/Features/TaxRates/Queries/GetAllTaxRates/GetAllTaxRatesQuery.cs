using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.TaxRates.Queries.GetAllTaxRates;

public record GetAllTaxRatesQuery : IRequest<GetAllTaxRatesResponse>;
