using MediatR;

namespace VendorManagementproj.Application.Features.Invoices.Commands.CreateInvoice;

public class CreateInvoiceCommand : IRequest<CreateInvoiceResponse>
{
    public int PurchaseOrderID { get; set; }
    public int VendorID { get; set; }
}