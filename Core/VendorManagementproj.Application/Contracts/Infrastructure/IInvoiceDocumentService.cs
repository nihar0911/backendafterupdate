using System.Threading.Tasks;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Infrastructure;

public interface IInvoiceDocumentService
{
    Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice);
}