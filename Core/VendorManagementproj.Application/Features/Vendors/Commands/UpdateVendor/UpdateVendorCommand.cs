using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Vendors.Commands.UpdateVendor;

public class UpdateVendorCommand : IRequest<UpdateVendorResponse>
{
    public int VendorID { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? GSTIN { get; set; }
    public string Status { get; set; } = string.Empty;
}
