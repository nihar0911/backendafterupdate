using MediatR;

namespace VendorManagementprojApplication.Features.VendorProducts.Commands.DeleteVendorProduct;

public record DeleteVendorProductCommand(int VendorProductID) : IRequest<DeleteVendorProductResponse>;
