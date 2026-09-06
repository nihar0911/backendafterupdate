using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Discounts.Queries.GetAllDiscounts;

public record GetAllDiscountsQuery : IRequest<GetAllDiscountsResponse>;
