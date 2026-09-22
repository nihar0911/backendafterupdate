using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.TaxRates.Queries.GetTaxRateById;

public record GetTaxRateByIdQuery(int TaxRateID) : IRequest<GetTaxRateByIdResponse>;
