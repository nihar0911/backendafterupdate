using MediatR;

namespace VendorManagementproj.Application.Features.Vendors.Commands.DeleteVendor;

public record DeleteVendorCommand(int VendorID) : IRequest<DeleteVendorResponse>;
