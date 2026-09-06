using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Discounts.Queries.GetDiscountById;

public class GetDiscountByIdQueryHandler : IRequestHandler<GetDiscountByIdQuery, GetDiscountByIdResponse>
{
    private readonly IDiscountRepository _repository;

    public GetDiscountByIdQueryHandler(IDiscountRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetDiscountByIdResponse> Handle(GetDiscountByIdQuery request, CancellationToken cancellationToken)
    {
        var discount = await _repository.GetByIdAsync(request.DiscountID);

        if (discount == null)
            return new GetDiscountByIdResponse { Discount = null };

        return new GetDiscountByIdResponse
        {
            Discount = new DiscountDto
            {
                DiscountID = discount.DiscountID,
                VendorID = discount.VendorID,
                ProductID = discount.ProductID,
                DiscountName = discount.DiscountName,
                DiscountType = discount.DiscountType,
                DiscountValue = discount.DiscountValue,
                MinimumQuantity = discount.MinimumQuantity,
                StartDate = discount.StartDate,
                EndDate = discount.EndDate,
                Status = discount.Status
            }
        };
    }
}
