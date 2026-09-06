using MediatR;

namespace VendorManagementprojApplication.Features.Discounts.Commands.DeleteDiscount;

public record DeleteDiscountCommand(int DiscountID) : IRequest<DeleteDiscountResponse>;
