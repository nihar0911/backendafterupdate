using System.ComponentModel.DataAnnotations;

namespace VendorManagementprojApplication.DTOs;

public class CreatePurchaseRequestItemDto
{
    [Required]
    public int ProductID { get; set; }

    [Required]
    [Range(0.01, 999999.99)]
    public decimal Quantity { get; set; }

    [Required]
    [StringLength(30)]
    public string Unit { get; set; } = string.Empty;
}
