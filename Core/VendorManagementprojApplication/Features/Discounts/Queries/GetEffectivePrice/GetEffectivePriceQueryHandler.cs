using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Discounts.Queries.GetEffectivePrice;

public class GetEffectivePriceQueryHandler : IRequestHandler<GetEffectivePriceQuery, GetEffectivePriceResponse>
{
    private readonly IDiscountRepository _repository;

    public GetEffectivePriceQueryHandler(IDiscountRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetEffectivePriceResponse> Handle(GetEffectivePriceQuery request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be greater than zero.");
        }

        var vendorProduct = await _repository.GetVendorProductAsync(request.VendorID, request.ProductID);

        if (vendorProduct == null)
        {
            throw new InvalidOperationException("The selected vendor does not supply the selected product.");
        }

        decimal originalPrice = vendorProduct.UnitPrice;

        var result = new DiscountPriceDto
        {
            VendorID = request.VendorID,
            ProductID = request.ProductID,
            OriginalUnitPrice = originalPrice,
            RequestedQuantity = request.Quantity,
            DiscountAmount = 0,
            EffectiveUnitPrice = originalPrice,
            DiscountApplied = false
        };

        var discount = await _repository.GetActiveDiscountAsync(request.VendorID, request.ProductID, DateTime.Today);

        if (discount == null)
        {
            return new GetEffectivePriceResponse { DiscountPrice = result };
        }

        result.DiscountType = discount.DiscountType;
        result.DiscountValue = discount.DiscountValue;
        result.MinimumQuantity = discount.MinimumQuantity;

        if (request.Quantity < discount.MinimumQuantity)
        {
            return new GetEffectivePriceResponse { DiscountPrice = result };
        }

        decimal discountAmount;

        if (discount.DiscountType.Equals("Percentage", StringComparison.OrdinalIgnoreCase))
        {
            discountAmount = originalPrice * discount.DiscountValue / 100;
        }
        else if (discount.DiscountType.Equals("FixedAmount", StringComparison.OrdinalIgnoreCase))
        {
            discountAmount = discount.DiscountValue;
        }
        else
        {
            throw new InvalidOperationException("Unsupported discount type.");
        }

        var effectivePrice = originalPrice - discountAmount;

        if (effectivePrice < 0)
        {
            effectivePrice = 0;
        }

        result.DiscountAmount = Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero);
        result.EffectiveUnitPrice = Math.Round(effectivePrice, 2, MidpointRounding.AwayFromZero);
        result.DiscountApplied = true;

        return new GetEffectivePriceResponse { DiscountPrice = result };
    }
}
