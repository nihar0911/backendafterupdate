using MediatR;

namespace VendorManagementprojApplication.Features.Contracts.Commands.EndContract;

public class EndContractCommand : IRequest<EndContractResponse>
{
    public int ContractID { get; set; }
}
