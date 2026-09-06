using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Discounts.Queries.GetAllDiscounts;

public class GetAllDiscountsQueryHandler : IRequestHandler<GetAllDiscountsQuery, GetAllDiscountsResponse>
{
    private readonly IDiscountRepository _repository;

    public GetAllDiscountsQueryHandler(IDiscountRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetAllDiscountsResponse> Handle(GetAllDiscountsQuery request, CancellationToken cancellationToken)
    {
        var discounts = await _repository.GetAllAsync();

        var list = discounts.Select(discount => new DiscountDto
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
        }).ToList();

        return new GetAllDiscountsResponse
        {
            Discounts = list
        };
    }
}
