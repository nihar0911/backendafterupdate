using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.TaxRates.Queries.GetAllTaxRates;

public record GetAllTaxRatesQuery : IRequest<GetAllTaxRatesResponse>;
