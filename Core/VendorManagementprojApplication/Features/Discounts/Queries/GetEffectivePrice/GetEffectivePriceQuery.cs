using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Discounts.Queries.GetEffectivePrice;

public record GetEffectivePriceQuery(int VendorID, int ProductID, decimal Quantity) : IRequest<GetEffectivePriceResponse>;
