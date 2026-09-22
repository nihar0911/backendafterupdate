using MediatR;

namespace VendorManagementproj.Application.Features.Contracts.Queries.GetContractEligibleVendors;

public class GetContractEligibleVendorsQuery : IRequest<GetContractEligibleVendorsResponse>
{
    public int OutletID { get; set; }
    public int ProductID { get; set; }

    public GetContractEligibleVendorsQuery() { }

    public GetContractEligibleVendorsQuery(int outletID, int productID)
    {
        OutletID = outletID;
        ProductID = productID;
    }
}
