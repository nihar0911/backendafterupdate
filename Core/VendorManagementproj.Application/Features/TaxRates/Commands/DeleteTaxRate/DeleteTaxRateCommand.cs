using MediatR;

namespace VendorManagementproj.Application.Features.TaxRates.Commands.DeleteTaxRate;

public record DeleteTaxRateCommand(int TaxRateID) : IRequest<DeleteTaxRateResponse>;
