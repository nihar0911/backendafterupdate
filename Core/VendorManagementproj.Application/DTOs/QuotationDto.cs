namespace VendorManagementprojApplication.DTOs;

public class QuotationDto
{
    public int QuotationID { get; set; }

    public int RequestID { get; set; }

    public int VendorID { get; set; }

    public DateTime ValidUntil { get; set; }

    public string Status { get; set; } = string.Empty;

    public List<QuotationItemDto> Items { get; set; }
        = new List<QuotationItemDto>();
}