using System;
using MediatR;

namespace VendorManagementproj.Application.Features.Invoices.Commands.MarkInvoicePaid;

public class MarkInvoicePaidCommand : IRequest<MarkInvoicePaidResponse>
{
    public int InvoiceID { get; set; }
    public int PaidByUserID { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime? PaymentDate { get; set; }
    public string? TransactionReference { get; set; }
}