using MediatR;

namespace VendorManagementproj.Application.Features.Contracts.Commands.EndContract;

public class EndContractCommand : IRequest<EndContractResponse>
{
    public int ContractID { get; set; }
}
