namespace VendorManagementprojApplication.Features.Contracts.Commands.RenewContract;

public class RenewContractProductItemDto
{
    public int ProductID { get; set; }
    public decimal ContractQuantity { get; set; }
    public decimal? UnitPrice { get; set; }
}
