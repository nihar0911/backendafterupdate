using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Persistence;

public interface IPurchaseRequestRepository
{
    Task<List<PurchaseRequest>> GetAllAsync();

    Task<PurchaseRequest?> GetByIdAsync(int requestID);

    Task<PurchaseRequest> AddAsync(PurchaseRequest purchaseRequest);

    Task<PurchaseRequest?> UpdateAsync(
        int requestID,
        PurchaseRequest purchaseRequest);

    Task<bool> DeleteAsync(int requestID);

    Task<List<PurchaseRequestItem>> GetItemsByRequestIdAsync(int requestID);

    Task<PurchaseRequestItem> AddItemAsync(PurchaseRequestItem item);

    Task<bool> DeleteItemAsync(int itemId);

    Task<List<PurchaseRequest>> GetByOutletIdAsync(int outletId);

    Task<List<PurchaseRequest>> GetByOutletIdsAsync(IEnumerable<int> outletIds);
}