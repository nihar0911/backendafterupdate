using MediatR;

namespace VendorManagementprojApplication.Features.TaxRates.Commands.DeleteTaxRate;

public record DeleteTaxRateCommand(int TaxRateID) : IRequest<DeleteTaxRateResponse>;
