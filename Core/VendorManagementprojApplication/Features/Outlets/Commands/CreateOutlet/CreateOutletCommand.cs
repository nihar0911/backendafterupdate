using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Outlets.Commands.CreateOutlet;

public class CreateOutletCommand : IRequest<CreateOutletResponse>
{
    public int OrganizationID { get; set; }
    public string OutletName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}
