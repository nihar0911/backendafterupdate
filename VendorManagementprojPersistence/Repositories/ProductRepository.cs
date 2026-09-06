using Microsoft.EntityFrameworkCore;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;
using VendorManagementprojPersistence.Data;

namespace VendorManagementprojPersistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly VendorManagementDbContext _context;

    public ProductRepository(VendorManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int productID)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.ProductID == productID);
    }

    public async Task<Product> AddAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> UpdateAsync(int productID, Product product)
    {
        var existingProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.ProductID == productID);

        if (existingProduct == null)
            return null;

        existingProduct.ProductName = product.ProductName;
        existingProduct.Category = product.Category;
        existingProduct.Unit = product.Unit;
        existingProduct.TaxRateID = product.TaxRateID;
        existingProduct.Status = product.Status;

        await _context.SaveChangesAsync();

        return existingProduct;
    }

    public async Task<bool> DeleteAsync(int productID)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.ProductID == productID);

        if (product == null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return true;
    }
}