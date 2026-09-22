using MediatR;

namespace VendorManagementproj.Application.Features.Contracts.Commands.UpdateContract;

public class UpdateContractCommand : IRequest<UpdateContractResponse>
{
    public int ContractID { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;
}