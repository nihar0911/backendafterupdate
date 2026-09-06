using MediatR;

namespace VendorManagementprojApplication.Features.Outlets.Commands.DeleteOutlet;

public record DeleteOutletCommand(int OutletID) : IRequest<DeleteOutletResponse>;
