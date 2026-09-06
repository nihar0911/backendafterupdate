using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommand : IRequest<UpdateProductResponse>
{
    public int ProductID { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int TaxRateID { get; set; }
    public string Status { get; set; } = string.Empty;
}
