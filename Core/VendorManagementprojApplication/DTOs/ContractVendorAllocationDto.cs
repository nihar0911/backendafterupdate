namespace VendorManagementprojApplication.DTOs;

public class ContractVendorAllocationDto
{
    public int ContractVendorAllocationID { get; set; }
    public int VendorID { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public decimal AllocationPercentage { get; set; }
    public decimal AllocatedQuantity { get; set; }
    public decimal UsedQuantity { get; set; }
    public string Status { get; set; } = string.Empty;
}
