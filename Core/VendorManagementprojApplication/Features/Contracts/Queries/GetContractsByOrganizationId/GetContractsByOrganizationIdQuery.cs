using MediatR;

namespace VendorManagementprojApplication.Features.Contracts.Queries.GetContractsByOrganizationId;

public class GetContractsByOrganizationIdQuery : IRequest<GetContractsByOrganizationIdResponse>
{
    public int OrganizationID { get; set; }

    public GetContractsByOrganizationIdQuery(int organizationID)
    {
        OrganizationID = organizationID;
    }
}
