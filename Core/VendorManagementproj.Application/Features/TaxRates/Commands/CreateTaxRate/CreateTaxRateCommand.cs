using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.TaxRates.Commands.CreateTaxRate;

public class CreateTaxRateCommand : IRequest<CreateTaxRateResponse>
{
    public string TaxName { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public string Status { get; set; } = string.Empty;
}
