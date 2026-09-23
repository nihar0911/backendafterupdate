namespace VendorManagementproj.Application.Features.Payments.Queries.GetPaymentReceiptPdf;

public class GetPaymentReceiptPdfResponse
{
    public byte[] PdfBytes { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
}
