using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Quotations.Commands.UpdateQuotation;

public class UpdateQuotationCommandHandler : IRequestHandler<UpdateQuotationCommand, UpdateQuotationResponse>
{
    private readonly IQuotationRepository _quotationRepository;
    private readonly IVendorProductRepository _vendorProductRepository;
    private readonly IDiscountRepository _discountRepository;
    private readonly ITaxRateRepository _taxRateRepository;
    private readonly IProductRepository _productRepository;

    public UpdateQuotationCommandHandler(
        IQuotationRepository quotationRepository,
        IVendorProductRepository vendorProductRepository,
        IDiscountRepository discountRepository,
        ITaxRateRepository taxRateRepository,
        IProductRepository productRepository)
    {
        _quotationRepository = quotationRepository;
        _vendorProductRepository = vendorProductRepository;
        _discountRepository = discountRepository;
        _taxRateRepository = taxRateRepository;
        _productRepository = productRepository;
    }

    public async Task<UpdateQuotationResponse> Handle(UpdateQuotationCommand request, CancellationToken cancellationToken)
    {
        var existingQuotation = await _quotationRepository.GetByIdAsync(request.QuotationID);

        if (existingQuotation == null)
            return new UpdateQuotationResponse { Quotation = null };

        if (request.Items == null || request.Items.Count == 0)
            throw new InvalidOperationException("Quotation must contain at least one item.");

        existingQuotation.VendorID = request.VendorID;
        existingQuotation.ValidUntil = request.ValidUntil;
        

        existingQuotation.QuotationItems.Clear();

        foreach (var itemDto in request.Items)
        {
            if (itemDto.Quantity <= 0)
                throw new InvalidOperationException("Quantity must be greater than zero.");

            var vendorProduct = await _vendorProductRepository.GetByVendorAndProductAsync(request.VendorID, itemDto.ProductID);

            if (vendorProduct == null)
                throw new InvalidOperationException($"Vendor does not supply ProductID {itemDto.ProductID}.");

            if (!string.Equals(vendorProduct.Status, "Active", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"ProductID {itemDto.ProductID} is not currently active for this vendor.");

            var product = await _productRepository.GetByIdAsync(itemDto.ProductID);

            if (product == null)
                throw new InvalidOperationException($"ProductID {itemDto.ProductID} does not exist.");

            var taxRate = await _taxRateRepository.GetByIdAsync(product.TaxRateID);

            if (taxRate == null)
                throw new InvalidOperationException($"No tax rate is configured for ProductID {itemDto.ProductID}.");

            if (!string.Equals(taxRate.Status, "Active", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"The tax rate assigned to ProductID {itemDto.ProductID} is not active.");

            var unitPrice = vendorProduct.UnitPrice;
            var grossAmount = unitPrice * itemDto.Quantity;
            var discount = await _discountRepository.GetActiveDiscountAsync(request.VendorID, itemDto.ProductID, DateTime.Now);

            decimal discountAmount = 0;

            if (discount != null && itemDto.Quantity >= discount.MinimumQuantity)
            {
                if (string.Equals(discount.DiscountType, "Percentage", StringComparison.OrdinalIgnoreCase))
                {
                    discountAmount = grossAmount * (discount.DiscountValue / 100m);
                }
                else if (string.Equals(discount.DiscountType, "Fixed", StringComparison.OrdinalIgnoreCase))
                {
                    discountAmount = discount.DiscountValue;
                }

                if (discountAmount > grossAmount)
                    discountAmount = grossAmount;
            }

            var taxableAmount = grossAmount - discountAmount;
            var taxAmount = taxableAmount * (taxRate.Percentage / 100m);
            var totalAmount = taxableAmount + taxAmount;

            existingQuotation.QuotationItems.Add(new QuotationItem
            {
                ProductID = itemDto.ProductID,
                Quantity = itemDto.Quantity,
                UnitPrice = unitPrice,
                DiscountAmount = discountAmount,
                TaxRate = taxRate.Percentage,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount
            });
        }

        var updatedQuotation = await _quotationRepository.UpdateAsync(request.QuotationID, existingQuotation);

        if (updatedQuotation == null)
            return new UpdateQuotationResponse { Quotation = null };

        return new UpdateQuotationResponse
        {
            Quotation = MapToDto(updatedQuotation)
        };
    }

    private static QuotationDto MapToDto(Quotation quotation)
    {
        return new QuotationDto
        {
            QuotationID = quotation.QuotationID,
            RequestID = quotation.RequestID,
            VendorID = quotation.VendorID,
            ValidUntil = quotation.ValidUntil,
            Status = quotation.Status,
            Items = quotation.QuotationItems.Select(item => new QuotationItemDto
            {
                QuotationItemID = item.QuotationItemID,
                ProductID = item.ProductID,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                DiscountAmount = item.DiscountAmount,
                TaxRate = item.TaxRate,
                TaxAmount = item.TaxAmount,
                TotalAmount = item.TotalAmount
            }).ToList()
        };
    }
}
