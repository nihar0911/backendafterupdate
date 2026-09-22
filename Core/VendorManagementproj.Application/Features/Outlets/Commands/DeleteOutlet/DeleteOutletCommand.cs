using MediatR;

namespace VendorManagementproj.Application.Features.Outlets.Commands.DeleteOutlet;

public record DeleteOutletCommand(int OutletID) : IRequest<DeleteOutletResponse>;
