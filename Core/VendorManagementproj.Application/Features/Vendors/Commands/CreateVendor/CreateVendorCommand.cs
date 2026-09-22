using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Vendors.Commands.CreateVendor;

public class CreateVendorCommand : IRequest<CreateVendorResponse>
{
    public string VendorName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? GSTIN { get; set; }
    public string Status { get; set; } = string.Empty;
}
