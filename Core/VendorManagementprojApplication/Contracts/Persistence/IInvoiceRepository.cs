using System.Collections.Generic;
using System.Threading.Tasks;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Persistence;

public interface IInvoiceRepository
{
    Task<Invoice> AddAsync(Invoice invoice);
    Task<Invoice?> GetByIdAsync(int invoiceID);
    Task<List<Invoice>> GetAllAsync();
    Task<Invoice?> GetByPurchaseOrderIdAsync(int purchaseOrderID);
    Task<Invoice?> UpdateAsync(Invoice invoice);
}