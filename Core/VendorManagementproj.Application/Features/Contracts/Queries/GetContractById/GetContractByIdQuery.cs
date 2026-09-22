using MediatR;

namespace VendorManagementprojApplication.Features.Contracts.Queries.GetContractById;

public class GetContractByIdQuery : IRequest<GetContractByIdResponse>
{
    public int ContractID { get; set; }

    public GetContractByIdQuery(int contractID)
    {
        ContractID = contractID;
    }
}
