using MediatR;

namespace VendorManagementproj.Application.Features.Invoices.Commands.RejectInvoice;

public class RejectInvoiceCommand : IRequest<RejectInvoiceResponse>
{
    public int InvoiceID { get; set; }
    public string Reason { get; set; } = string.Empty;
}