using MediatR;

namespace VendorManagementprojApplication.Features.Contracts.Commands.ResetContract;

public class ResetContractCommand : IRequest<ResetContractResponse>
{
    public int ContractID { get; set; }
}