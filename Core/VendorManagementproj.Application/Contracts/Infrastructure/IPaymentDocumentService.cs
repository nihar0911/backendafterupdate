using System.Threading.Tasks;
using VendorManagementproj.Domain.Entities;

namespace VendorManagementproj.Application.Contracts.Infrastructure;

public interface IPaymentDocumentService
{
    Task<byte[]> GeneratePaymentReceiptPdfAsync(Payment payment);
}
