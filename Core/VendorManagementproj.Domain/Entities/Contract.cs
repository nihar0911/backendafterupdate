namespace VendorManagementproj.Domain.Entities;

public class Contract
{
    public int ContractID { get; set; }
    public int? QuotationID { get; set; }
    public int OutletID { get; set; }
    public int? VendorID { get; set; }
    public int ProductID { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal UsedQuantity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public Quotation? Quotation { get; set; }
    public Outlet? Outlet { get; set; }
    public Product? Product { get; set; }
    public virtual Vendor? Vendor { get; set; }

    // Phase 1 Target Architecture: One Contract has many ContractProducts
    public virtual ICollection<ContractProduct> ContractProducts { get; set; } = new List<ContractProduct>();

    // Legacy multi-vendor allocation collection preserved for Phase 1 backward compatibility
    public virtual ICollection<ContractVendorAllocation> VendorAllocations { get; set; } = new List<ContractVendorAllocation>();
}
