using System.ComponentModel.DataAnnotations;

namespace VendorManagementprojApplication.DTOs;

public class UpdateOutletDto
{
    [Required]
    public int OrganizationID { get; set; }

    [Required]
    [MaxLength(150)]
    public string OutletName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Address { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }
}