using MediatR;

namespace VendorManagementprojApplication.Features.Invoices.Queries.GetInvoiceById;

public class GetInvoiceByIdQuery : IRequest<GetInvoiceByIdResponse>
{
    public int InvoiceID { get; set; }
}