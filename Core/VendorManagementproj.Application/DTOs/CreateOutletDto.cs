using System.ComponentModel.DataAnnotations;

namespace VendorManagementproj.Application.DTOs;

public class CreateOutletDto
{
    [Required]
    public int OrganizationID { get; set; }

    [Required]
    [MaxLength(150)]
    public string OutletName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Address { get; set; }
}