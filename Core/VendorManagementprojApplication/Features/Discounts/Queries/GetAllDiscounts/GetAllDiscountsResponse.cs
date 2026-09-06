using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Discounts.Queries.GetAllDiscounts;

public class GetAllDiscountsResponse
{
    public List<DiscountDto> Discounts { get; set; } = new();
}
