namespace VendorManagementprojApplication.Contracts.Infrastructure;

public interface IInvoiceDocumentService
{
    Task<byte[]> GenerateInvoicePdfAsync(
        int invoiceID,
        int purchaseOrderID,
        decimal subtotal,
        decimal taxAmount,
        decimal totalAmount);
}