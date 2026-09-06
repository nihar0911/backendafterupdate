namespace VendorManagementprojApplication.DTOs;

public class ComplaintDto
{
    public int ComplaintID { get; set; }

    public int VendorID { get; set; }

    public int OutletID { get; set; }

    public int PurchaseOrderID { get; set; }

    public int POItemID { get; set; }

    public int ProductID { get; set; }

    public int RaisedByUserID { get; set; }

    public DateTime ComplaintDate { get; set; }

    public string ComplaintType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}