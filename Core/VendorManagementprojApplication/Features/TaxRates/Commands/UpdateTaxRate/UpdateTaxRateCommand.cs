using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.TaxRates.Commands.UpdateTaxRate;

public class UpdateTaxRateCommand : IRequest<UpdateTaxRateResponse>
{
    public int TaxRateID { get; set; }
    public string TaxName { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public string Status { get; set; } = string.Empty;
}
