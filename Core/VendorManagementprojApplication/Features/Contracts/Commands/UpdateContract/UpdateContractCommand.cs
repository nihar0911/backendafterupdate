using MediatR;

namespace VendorManagementprojApplication.Features.Contracts.Commands.UpdateContract;

public class UpdateContractCommand : IRequest<UpdateContractResponse>
{
    public int ContractID { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;
}