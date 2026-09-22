using System.Collections.Generic;
using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Contracts.Commands.CreateContractFromQuotation;

public class CreateContractFromQuotationCommand : IRequest<CreateContractFromQuotationResponse>
{
    public int QuotationID { get; set; }
    public List<CreateContractVendorAllocationDto>? Allocations { get; set; }
}
