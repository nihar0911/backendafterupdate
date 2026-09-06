using MediatR;

namespace VendorManagementprojApplication.Features.VendorProducts.Commands.UpdateVendorProduct;

public class UpdateVendorProductCommand : IRequest<UpdateVendorProductResponse>
{
    public int VendorProductID { get; set; }
    public int VendorID { get; set; }
    public int ProductID { get; set; }
    public decimal UnitPrice { get; set; }
    public int EstimatedDeliveryDays { get; set; }
    public string Status { get; set; } = string.Empty;
}