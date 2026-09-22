using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Persistence;

public interface ITaxRateRepository
{
    Task<List<TaxRate>> GetAllAsync();

    Task<TaxRate?> GetByIdAsync(int taxRateID);

    Task<TaxRate?> GetActiveTaxRateAsync();

    Task<TaxRate> AddAsync(TaxRate taxRate);

    Task<TaxRate?> UpdateAsync(
        int taxRateID,
        TaxRate taxRate);

    Task<bool> DeleteAsync(int taxRateID);
}