using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Complaints.Commands.CreateComplaint;

public class CreateComplaintCommandHandler
    : IRequestHandler<CreateComplaintCommand, CreateComplaintResponse>
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateComplaintCommandHandler(
        IComplaintRepository complaintRepository,
        IPurchaseOrderRepository purchaseOrderRepository,
        IUserRepository userRepository,
        IVendorRepository vendorRepository,
        ICurrentUserService currentUserService)
    {
        _complaintRepository = complaintRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _userRepository = userRepository;
        _vendorRepository = vendorRepository;
        _currentUserService = currentUserService;
    }

    public async Task<CreateComplaintResponse> Handle(
        CreateComplaintCommand request,
        CancellationToken cancellationToken)
    {
        if (request.VendorID <= 0)
            throw new InvalidOperationException(
                "A valid vendor is required.");

        if (request.OutletID <= 0)
            throw new InvalidOperationException(
                "A valid outlet is required.");

        if (request.PurchaseOrderID <= 0)
            throw new InvalidOperationException(
                "A valid purchase order is required.");

        if (request.POItemID <= 0)
            throw new InvalidOperationException(
                "A valid purchase order item is required.");

        if (request.ProductID <= 0)
            throw new InvalidOperationException(
                "A valid product is required.");

        int raisedByUserId = _currentUserService.UserID ?? request.RaisedByUserID;

        if (raisedByUserId <= 0)
            throw new InvalidOperationException(
                "A valid user is required.");

        if (string.IsNullOrWhiteSpace(request.ComplaintType))
            throw new InvalidOperationException(
                "Complaint type is required.");

        if (string.IsNullOrWhiteSpace(request.Description))
            throw new InvalidOperationException(
                "Complaint description is required.");

        var vendor =
            await _vendorRepository.GetByIdAsync(request.VendorID);

        if (vendor == null)
            throw new InvalidOperationException(
                "Vendor does not exist.");

        var user =
            await _userRepository.GetByIdAsync(raisedByUserId);

        if (user == null)
            throw new InvalidOperationException(
                "User does not exist.");

        if (user.OutletID == null)
            throw new InvalidOperationException(
                "User is not assigned to an outlet.");

        if (user.OutletID != request.OutletID)
            throw new UnauthorizedAccessException(
                "User is not authorized to raise a complaint for this outlet.");

        var purchaseOrder =
            await _purchaseOrderRepository
                .GetByIdAsync(request.PurchaseOrderID);

        if (purchaseOrder == null)
            throw new InvalidOperationException(
                "Purchase order does not exist.");

        if (purchaseOrder.OutletID != request.OutletID)
            throw new InvalidOperationException(
                "Purchase order does not belong to this outlet.");

        if (purchaseOrder.VendorID != request.VendorID)
            throw new InvalidOperationException(
                "Purchase order does not belong to this vendor.");

        var purchaseOrderItem =
            purchaseOrder.Items.FirstOrDefault(
                item => item.POItemID == request.POItemID);

        if (purchaseOrderItem == null)
            throw new InvalidOperationException(
                "Purchase order item does not belong to this purchase order.");

        if (purchaseOrderItem.ProductID != request.ProductID)
            throw new InvalidOperationException(
                "Product does not match the purchase order item.");

        if (!string.Equals(
                purchaseOrder.Status,
                "Delivered",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "A complaint can only be raised after the purchase order has been delivered.");
        }

        var severity = request.Severity?.Trim();

        if (!string.Equals(severity, "Low", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(severity, "Medium", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(severity, "High", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(severity, "Critical", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Severity must be Low, Medium, High, or Critical.");
        }

        var complaint = new Complaint
        {
            VendorID = request.VendorID,
            OutletID = request.OutletID,
            PurchaseOrderID = request.PurchaseOrderID,
            POItemID = request.POItemID,
            ProductID = request.ProductID,
            RaisedByUserID = raisedByUserId,
            ComplaintDate = DateTime.Now,
            ComplaintType = request.ComplaintType.Trim(),
            Description = request.Description.Trim(),
            Severity = severity!,
            Status = "Active"
        };

        var createdComplaint =
            await _complaintRepository.AddAsync(complaint);

        return new CreateComplaintResponse
        {
            Complaint = MapToDto(createdComplaint)
        };
    }

    private static ComplaintDto MapToDto(
        Complaint complaint)
    {
        return new ComplaintDto
        {
            ComplaintID = complaint.ComplaintID,
            VendorID = complaint.VendorID,
            OutletID = complaint.OutletID,
            PurchaseOrderID = complaint.PurchaseOrderID,
            POItemID = complaint.POItemID,
            ProductID = complaint.ProductID,
            RaisedByUserID = complaint.RaisedByUserID,
            ComplaintDate = complaint.ComplaintDate,
            ComplaintType = complaint.ComplaintType,
            Description = complaint.Description,
            Severity = complaint.Severity,
            Status = complaint.Status
        };
    }
}