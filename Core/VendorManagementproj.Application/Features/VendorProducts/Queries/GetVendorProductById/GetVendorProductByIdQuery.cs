using MediatR;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.VendorProducts.Queries.GetVendorProductById;

public record GetVendorProductByIdQuery(int VendorProductID) : IRequest<GetVendorProductByIdResponse>;
