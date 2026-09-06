using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.TaxRates.Queries.GetTaxRateById;

public record GetTaxRateByIdQuery(int TaxRateID) : IRequest<GetTaxRateByIdResponse>;
