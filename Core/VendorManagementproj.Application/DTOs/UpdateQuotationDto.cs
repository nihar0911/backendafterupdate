namespace VendorManagementprojApplication.DTOs;

public class UpdateQuotationDto
{
    public int VendorID { get; set; }

    public DateTime ValidUntil { get; set; }

    public string Status { get; set; } = string.Empty;

    public List<CreateQuotationItemDto> Items { get; set; }
        = new List<CreateQuotationItemDto>();
}