using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Discounts.Commands.UpdateDiscount;

public class UpdateDiscountCommandHandler : IRequestHandler<UpdateDiscountCommand, UpdateDiscountResponse>
{
    private readonly IDiscountRepository _repository;

    public UpdateDiscountCommandHandler(IDiscountRepository repository)
    {
        _repository = repository;
    }

    public async Task<UpdateDiscountResponse> Handle(UpdateDiscountCommand request, CancellationToken cancellationToken)
    {
        await ValidateDiscountAsync(
            request.VendorID,
            request.ProductID,
            request.DiscountType,
            request.DiscountValue,
            request.MinimumQuantity,
            request.StartDate,
            request.EndDate,
            request.DiscountID);

        var discount = new Discount
        {
            VendorID = request.VendorID,
            ProductID = request.ProductID,
            DiscountName = request.DiscountName,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MinimumQuantity = request.MinimumQuantity,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = request.Status
        };

        var updatedDiscount = await _repository.UpdateAsync(request.DiscountID, discount);

        if (updatedDiscount == null)
            return new UpdateDiscountResponse { Discount = null };

        return new UpdateDiscountResponse
        {
            Discount = new DiscountDto
            {
                DiscountID = updatedDiscount.DiscountID,
                VendorID = updatedDiscount.VendorID,
                ProductID = updatedDiscount.ProductID,
                DiscountName = updatedDiscount.DiscountName,
                DiscountType = updatedDiscount.DiscountType,
                DiscountValue = updatedDiscount.DiscountValue,
                MinimumQuantity = updatedDiscount.MinimumQuantity,
                StartDate = updatedDiscount.StartDate,
                EndDate = updatedDiscount.EndDate,
                Status = updatedDiscount.Status
            }
        };
    }

    private async Task ValidateDiscountAsync(
        int vendorID,
        int productID,
        string discountType,
        decimal discountValue,
        decimal minimumQuantity,
        DateTime startDate,
        DateTime endDate,
        int? excludeDiscountID)
    {
        var relationshipExists = await _repository.VendorProductExistsAsync(vendorID, productID);
        if (!relationshipExists)
            throw new InvalidOperationException("The selected vendor does not supply the selected product.");

        if (string.IsNullOrWhiteSpace(discountType))
            throw new InvalidOperationException("Discount type is required.");

        if (discountValue <= 0)
            throw new InvalidOperationException("Discount value must be greater than zero.");

        if (discountType.Equals("Percentage", StringComparison.OrdinalIgnoreCase) && discountValue > 100)
            throw new InvalidOperationException("Percentage discount cannot be greater than 100.");

        if (discountType.Equals("FixedAmount", StringComparison.OrdinalIgnoreCase))
        {
            var vendorProduct = await _repository.GetVendorProductAsync(vendorID, productID);
            if (vendorProduct != null && discountValue > vendorProduct.UnitPrice)
                throw new InvalidOperationException("Fixed discount cannot be greater than the product unit price.");
        }

        if (minimumQuantity <= 0)
            throw new InvalidOperationException("Minimum quantity must be greater than zero.");

        if (endDate < startDate)
            throw new InvalidOperationException("End date cannot be before start date.");

        var overlappingDiscount = await _repository.HasOverlappingDiscountAsync(
            vendorID,
            productID,
            startDate,
            endDate,
            excludeDiscountID);

        if (overlappingDiscount)
            throw new InvalidOperationException("An existing discount already overlaps this date range for the selected vendor and product.");
    }
}
