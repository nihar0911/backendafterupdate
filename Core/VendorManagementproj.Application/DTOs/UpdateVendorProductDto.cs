using System.ComponentModel.DataAnnotations;

namespace VendorManagementprojApplication.DTOs;

public class UpdateVendorProductDto
{
    [Required]
    public int VendorID { get; set; }

    [Required]
    public int ProductID { get; set; }

    [Required]
    public decimal UnitPrice { get; set; }

    [Required]
    public int EstimatedDeliveryDays { get; set; }

    [Required]
    public string Status { get; set; } = string.Empty;
}