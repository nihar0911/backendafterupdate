using MediatR;

namespace VendorManagementproj.Application.Features.Payments.Queries.GetPaymentReceiptPdf;

public class GetPaymentReceiptPdfQuery : IRequest<GetPaymentReceiptPdfResponse>
{
    public int PaymentID { get; set; }
}
