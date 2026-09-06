namespace VendorManagementprojApplication.DTOs;

public class CreateQuotationDto
{
    public int RequestID { get; set; }

    public int VendorID { get; set; }

    public DateTime ValidUntil { get; set; }

    public string Status { get; set; } = string.Empty;

    public List<CreateQuotationItemDto> Items { get; set; }
        = new List<CreateQuotationItemDto>();
}