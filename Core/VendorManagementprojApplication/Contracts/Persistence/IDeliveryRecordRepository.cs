using System.Collections.Generic;
using System.Threading.Tasks;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Persistence;

public interface IDeliveryRecordRepository
{
    Task<DeliveryRecord> AddAsync(DeliveryRecord deliveryRecord);
    Task<DeliveryRecord?> GetByIdAsync(int deliveryRecordID);
    Task<List<DeliveryRecord>> GetAllAsync();
    Task<List<DeliveryRecord>> GetByPurchaseOrderIdAsync(int purchaseOrderID);
    Task<DeliveryRecord?> UpdateAsync(DeliveryRecord deliveryRecord);
    Task<List<DeliveryRecord>> GetByVendorProductOutletAsync(int vendorID, int productID, int outletID);
    Task<List<DeliveryRecord>> GetConfirmedByVendorAsync(int vendorID);
    Task<List<DeliveryRecord>> GetConfirmedByVendorAndProductAsync(int vendorID, int productID);
}

