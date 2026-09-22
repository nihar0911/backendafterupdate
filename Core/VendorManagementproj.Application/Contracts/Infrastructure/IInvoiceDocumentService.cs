using System.Threading.Tasks;
using VendorManagementproj.Domain.Entities;

namespace VendorManagementproj.Application.Contracts.Infrastructure;

public interface IInvoiceDocumentService
{
    Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice);
}