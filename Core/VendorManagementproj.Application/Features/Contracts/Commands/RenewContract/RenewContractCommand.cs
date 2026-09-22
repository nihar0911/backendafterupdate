using System;
using System.Collections.Generic;
using MediatR;

namespace VendorManagementprojApplication.Features.Contracts.Commands.RenewContract;

public class RenewContractCommand : IRequest<RenewContractResponse>
{
    public int ContractID { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? PaymentMethod { get; set; }
    public List<RenewContractProductItemDto>? Products { get; set; }
}
