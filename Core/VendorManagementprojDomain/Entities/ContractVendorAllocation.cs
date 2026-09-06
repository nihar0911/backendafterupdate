namespace VendorManagementprojDomain.Entities;

public class ContractVendorAllocation
{
    public int ContractVendorAllocationID { get; set; }
    public int ContractID { get; set; }
    public int VendorID { get; set; }
    public decimal AllocationPercentage { get; set; }
    public decimal AllocatedQuantity { get; set; }
    public decimal UsedQuantity { get; set; }
    public string Status { get; set; } = string.Empty;

    public virtual Contract? Contract { get; set; }
    public virtual Vendor? Vendor { get; set; }
}