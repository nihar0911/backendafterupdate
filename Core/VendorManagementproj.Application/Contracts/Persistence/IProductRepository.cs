using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Persistence;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();

    Task<Product?> GetByIdAsync(int productID);

    Task<Product> AddAsync(Product product);

    Task<Product?> UpdateAsync(
        int productID,
        Product product);

    Task<bool> DeleteAsync(int productID);
}