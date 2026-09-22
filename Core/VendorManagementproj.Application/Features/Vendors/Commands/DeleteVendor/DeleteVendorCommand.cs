using MediatR;

namespace VendorManagementprojApplication.Features.Vendors.Commands.DeleteVendor;

public record DeleteVendorCommand(int VendorID) : IRequest<DeleteVendorResponse>;
