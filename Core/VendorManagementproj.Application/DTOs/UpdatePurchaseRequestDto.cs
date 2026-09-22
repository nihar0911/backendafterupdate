namespace VendorManagementprojApplication.DTOs;

public class UpdatePurchaseRequestDto
{
    public int OutletID { get; set; }

    public int CreatedByUserID { get; set; }

    public DateTime RequestDate { get; set; }

    public string Status { get; set; } = string.Empty;
}