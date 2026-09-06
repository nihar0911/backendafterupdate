
using System.ComponentModel.DataAnnotations;

namespace VendorManagementprojApplication.DTOs;

public class UpdateOrganizationDto
{
    [Required]
    [MaxLength(150)]
    public string OrganizationName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Address { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(150)]
    [EmailAddress]
    public string? Email { get; set; }
}
