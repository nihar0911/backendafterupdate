using MediatR;

namespace VendorManagementproj.Application.Features.Invoices.Commands.ApproveInvoice;

public class ApproveInvoiceCommand : IRequest<ApproveInvoiceResponse>
{
    public int InvoiceID { get; set; }
}