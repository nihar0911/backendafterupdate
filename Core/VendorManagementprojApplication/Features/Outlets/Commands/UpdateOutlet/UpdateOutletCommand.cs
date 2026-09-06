using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Outlets.Commands.UpdateOutlet;

public class UpdateOutletCommand : IRequest<UpdateOutletResponse>
{
    public int OutletID { get; set; }
    public int OrganizationID { get; set; }
    public string OutletName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}
