using MediatR;

namespace VendorManagementprojApplication.Features.Contracts.Queries.GetContractsByOutlet;

public class GetContractsByOutletQuery : IRequest<GetContractsByOutletResponse>
{
    public int OutletID { get; set; }

    public GetContractsByOutletQuery(int outletID)
    {
        OutletID = outletID;
    }
}
