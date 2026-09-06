using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Quotations.Commands.CreateQuotation;

public class CreateQuotationCommand : IRequest<CreateQuotationResponse>
{
    public int RequestID { get; set; }
    public int VendorID { get; set; }
    public DateTime ValidUntil { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<CreateQuotationItemDto> Items { get; set; } = new();
}
