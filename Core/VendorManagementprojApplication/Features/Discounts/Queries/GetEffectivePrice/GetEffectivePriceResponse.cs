using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Discounts.Queries.GetEffectivePrice;

public class GetEffectivePriceResponse
{
    public DiscountPriceDto DiscountPrice { get; set; } = null!;
}
