using System.Collections.Generic;
using System.Threading.Tasks;
using VendorManagementproj.Domain.Entities;

namespace VendorManagementproj.Application.Contracts.Persistence;

public interface IPaymentRepository
{
    Task<Payment> AddAsync(Payment payment);
    Task<Payment?> GetByIdAsync(int paymentID);
    Task<Payment?> GetByInvoiceIdAsync(int invoiceID);
    Task<List<Payment>> GetAllAsync();
    Task<List<Payment>> GetByOrganizationIdAsync(int organizationID);
    Task<List<Payment>> GetByVendorIdAsync(int vendorID);
}