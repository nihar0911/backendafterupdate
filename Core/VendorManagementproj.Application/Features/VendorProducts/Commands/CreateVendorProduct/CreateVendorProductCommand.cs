using MediatR;

namespace VendorManagementproj.Application.Features.VendorProducts.Commands.CreateVendorProduct;

public class CreateVendorProductCommand : IRequest<CreateVendorProductResponse>
{
    public int VendorID { get; set; }
    public int ProductID { get; set; }
    public decimal UnitPrice { get; set; }
    public int EstimatedDeliveryDays { get; set; }
    public string Status { get; set; } = string.Empty;
}