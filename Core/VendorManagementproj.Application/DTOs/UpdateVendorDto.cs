namespace VendorManagementproj.Application.DTOs;

public class UpdateVendorDto
{
    public string VendorName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? GSTIN { get; set; }
    public string Status { get; set; } = string.Empty;
}