namespace VendorManagementproj.Domain.Entities;

public class ContractProduct
{
    public int ContractProductID { get; set; }
    public int ContractID { get; set; }
    public int ProductID { get; set; }
    public decimal ContractQuantity { get; set; }
    public decimal PurchasedQuantity { get; set; }
    public decimal? UnitPrice { get; set; }

    public virtual Contract? Contract { get; set; }
    public virtual Product? Product { get; set; }
}
