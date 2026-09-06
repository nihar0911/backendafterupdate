using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Persistence;

public interface IDiscountRepository
{
    Task<List<Discount>> GetAllAsync();
    Task<Discount?> GetByIdAsync(int discountID);
    Task<Discount> AddAsync(Discount discount);
    Task<Discount?> UpdateAsync(int discountID, Discount discount);
    Task<bool> DeleteAsync(int discountID);

    Task<bool> VendorProductExistsAsync(
        int vendorID,
        int productID);

    Task<VendorProduct?> GetVendorProductAsync(
        int vendorID,
        int productID);

    Task<Discount?> GetActiveDiscountAsync(
        int vendorID,
        int productID,
        DateTime date);

    Task<bool> HasOverlappingDiscountAsync(
        int vendorID,
        int productID,
        DateTime startDate,
        DateTime endDate,
        int? excludeDiscountID = null);
}