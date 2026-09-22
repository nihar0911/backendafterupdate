using System.Collections.Generic;
using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Contracts.Commands.CreateContractFromQuotation;

public class CreateContractFromQuotationCommand : IRequest<CreateContractFromQuotationResponse>
{
    public int QuotationID { get; set; }
    public List<CreateContractVendorAllocationDto>? Allocations { get; set; }
}
