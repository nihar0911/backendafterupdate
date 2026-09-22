namespace VendorManagementprojApplication.DTOs;

public class CreateQuotationItemDto
{
    public int ProductID { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}