namespace VendorManagementprojApplication.DTOs;

public class ContractProductAssignmentDto
{
    public int ProductID { get; set; }
    public int VendorID { get; set; }
    public decimal ContractQuantity { get; set; }
    public decimal? UnitPrice { get; set; }
}
