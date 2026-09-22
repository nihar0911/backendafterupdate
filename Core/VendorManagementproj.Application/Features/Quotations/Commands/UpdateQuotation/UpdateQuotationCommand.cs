using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Quotations.Commands.UpdateQuotation;

public class UpdateQuotationCommand : IRequest<UpdateQuotationResponse>
{
    public int QuotationID { get; set; }

    public int VendorID { get; set; }

    public DateTime ValidUntil { get; set; }

    public List<CreateQuotationItemDto> Items { get; set; } = new();
}