namespace VendorManagementprojApplication.DTOs;

public class ContractVendorAllocationDto
{
    public int ContractVendorAllocationID { get; set; }
    public int VendorID { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public decimal AllocationPercentage { get; set; }
    public decimal AllocatedQuantity { get; set; }
    public decimal UsedQuantity { get; set; }
    public decimal RemainingQuantity => Math.Max(AllocatedQuantity - UsedQuantity, 0m);
    public decimal ExtraOrderQuantity => Math.Max(UsedQuantity - AllocatedQuantity, 0m);
    public string Status { get; set; } = string.Empty;
}
