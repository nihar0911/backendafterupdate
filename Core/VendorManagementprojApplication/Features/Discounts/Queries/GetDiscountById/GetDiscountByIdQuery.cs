using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Discounts.Queries.GetDiscountById;

public record GetDiscountByIdQuery(int DiscountID) : IRequest<GetDiscountByIdResponse>;
