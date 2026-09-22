using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Persistence;

public interface IQuotationRepository
{
    Task<List<Quotation>> GetAllAsync();
    Task<Quotation?> GetByIdAsync(int quotationID);
    Task<List<Quotation>> GetByRequestAndVendorAsync(int requestId, int vendorId);
    Task<Quotation> AddAsync(Quotation quotation);
    Task<Quotation?> UpdateAsync(int quotationID, Quotation quotation);
    Task<bool> DeleteAsync(int quotationID);
    Task<List<Quotation>> GetByOutletIdAsync(int outletId);
    Task<List<Quotation>> GetByOutletIdsAsync(IEnumerable<int> outletIds);
    Task<List<Quotation>> GetByVendorIdAsync(int vendorId);
}