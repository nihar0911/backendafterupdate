using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Quotations.Commands.UpdateQuotation;

public class UpdateQuotationCommand : IRequest<UpdateQuotationResponse>
{
    public int QuotationID { get; set; }

    public int VendorID { get; set; }

    public DateTime ValidUntil { get; set; }

    public List<CreateQuotationItemDto> Items { get; set; } = new();
}