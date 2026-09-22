using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Persistence;

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder> AddAsync(PurchaseOrder purchaseOrder);
    Task<PurchaseOrder?> GetByIdAsync(int purchaseOrderID);
    Task<List<PurchaseOrder>> GetAllAsync();
    Task<List<PurchaseOrder>> GetPendingByVendorAsync(int vendorID);
    Task<PurchaseOrder?> UpdateAsync(PurchaseOrder purchaseOrder);
    Task<PurchaseOrder?> GetByQuotationIdAsync(int quotationID);
    Task<bool> DeleteAsync(int purchaseOrderID);
    Task<List<PurchaseOrder>> GetByOutletIdAsync(int outletId);
    Task<List<PurchaseOrder>> GetByOutletIdsAsync(IEnumerable<int> outletIds);
    Task<List<PurchaseOrder>> GetByVendorIdAsync(int vendorId);
}