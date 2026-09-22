namespace VendorManagementprojApplication.DTOs;

public class CreateContractDto
{
    public int OutletID { get; set; }
    public int ProductID { get; set; }
    public decimal TotalQuantity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public List<CreateContractVendorAllocationDto> Allocations { get; set; } = new();
}