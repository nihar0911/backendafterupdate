using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Persistence;

public interface IVendorProductRepository
{
    Task<List<VendorProduct>> GetAllAsync();

    Task<VendorProduct?> GetByIdAsync(
        int vendorProductID);

    Task<VendorProduct?> GetByVendorAndProductAsync(
        int vendorID,
        int productID);

    Task<List<VendorProduct>> GetByProductNameAsync(
        string productName);

    Task<VendorProduct> AddAsync(
        VendorProduct vendorProduct);

    Task<VendorProduct?> UpdateAsync(
        int vendorProductID,
        VendorProduct vendorProduct);

    Task<bool> DeleteAsync(
        int vendorProductID);
    /*  Task<List<VendorProduct>> GetByProductNameForOutletAsync(
      string productName,
      int outletID);*/
    Task<List<VendorProduct>> GetEligibleVendorsForProductAsync(
      int productID,
      int outletID);
    Task<List<VendorProduct>> GetByProductNameForOutletAsync(
    string productName,
    int outletID);
    Task<bool> IsVendorEligibleAsync(
    int vendorID,
    int productID,
    int outletID);
}