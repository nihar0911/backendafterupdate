using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommand : IRequest<CreateProductResponse>
{
    public string ProductName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int TaxRateID { get; set; }
    public string Status { get; set; } = string.Empty;
}
