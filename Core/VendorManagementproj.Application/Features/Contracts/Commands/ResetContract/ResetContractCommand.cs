using MediatR;

namespace VendorManagementproj.Application.Features.Contracts.Commands.ResetContract;

public class ResetContractCommand : IRequest<ResetContractResponse>
{
    public int ContractID { get; set; }
    public decimal? NewTotalQuantity { get; set; }
}