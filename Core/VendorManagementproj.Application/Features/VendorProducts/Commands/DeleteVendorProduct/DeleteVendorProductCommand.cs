using MediatR;

namespace VendorManagementproj.Application.Features.VendorProducts.Commands.DeleteVendorProduct;

public record DeleteVendorProductCommand(int VendorProductID) : IRequest<DeleteVendorProductResponse>;
