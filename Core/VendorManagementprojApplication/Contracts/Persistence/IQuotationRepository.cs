using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Persistence;

public interface IQuotationRepository
{
    Task<List<Quotation>> GetAllAsync();
    Task<Quotation?> GetByIdAsync(int quotationID);
    Task<Quotation> AddAsync(Quotation quotation);
    Task<Quotation?> UpdateAsync(int quotationID, Quotation quotation);
    Task<bool> DeleteAsync(int quotationID);
}