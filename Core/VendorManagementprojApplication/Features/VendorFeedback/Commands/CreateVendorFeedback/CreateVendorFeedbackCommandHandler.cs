using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;
using VendorFeedbackEntity = VendorManagementprojDomain.Entities.VendorFeedback;

namespace VendorManagementprojApplication.Features.VendorFeedback.Commands.CreateVendorFeedback;

public class CreateVendorFeedbackCommandHandler
    : IRequestHandler<CreateVendorFeedbackCommand, CreateVendorFeedbackResponse>
{
    private readonly IVendorFeedbackRepository _feedbackRepository;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationRepository _notificationRepository;

    public CreateVendorFeedbackCommandHandler(
        IVendorFeedbackRepository feedbackRepository,
        IPurchaseOrderRepository purchaseOrderRepository,
        IUserRepository userRepository,
        IVendorRepository vendorRepository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService,
        INotificationRepository notificationRepository)
    {
        _feedbackRepository = feedbackRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _userRepository = userRepository;
        _vendorRepository = vendorRepository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
        _notificationRepository = notificationRepository;
    }

    public async Task<CreateVendorFeedbackResponse> Handle(
        CreateVendorFeedbackCommand request,
        CancellationToken cancellationToken)
    {
        if (request.VendorID <= 0)
            throw new InvalidOperationException("A valid vendor is required.");

        if (request.OutletID <= 0)
            throw new InvalidOperationException("A valid outlet is required.");

        if (request.PurchaseOrderID <= 0)
            throw new InvalidOperationException("A valid purchase order is required.");

        if (request.POItemID <= 0)
            throw new InvalidOperationException("A valid purchase order item is required.");

        int ratedByUserId = _currentUserService.UserID ?? request.RatedByUserID;
        if (ratedByUserId <= 0)
            throw new InvalidOperationException("A valid user is required.");

        if (request.Rating < 1 || request.Rating > 5)
            throw new InvalidOperationException("Overall rating must be between 1 and 5.");

        if (request.ProductQualityRating < 1 || request.ProductQualityRating > 5)
            throw new InvalidOperationException("Product quality rating must be between 1 and 5.");

        if (request.DeliveryRating < 1 || request.DeliveryRating > 5)
            throw new InvalidOperationException("Delivery rating must be between 1 and 5.");

        if (string.IsNullOrWhiteSpace(request.Review))
            throw new InvalidOperationException("Review is required.");

        var vendor = await _vendorRepository.GetByIdAsync(request.VendorID);
        if (vendor == null)
            throw new InvalidOperationException("Vendor does not exist.");

        var user = await _userRepository.GetByIdAsync(ratedByUserId);
        if (user == null)
            throw new InvalidOperationException("User does not exist.");

        var outlet = await _outletRepository.GetByIdAsync(request.OutletID);
        if (outlet == null)
            throw new InvalidOperationException("Outlet does not exist.");

        // Authorization check: Organization Manager (by Org), Outlet Manager (by Outlet), or Purchase Manager (by Outlet)
        bool isAuthorized = false;
        if (string.Equals(user.Role?.RoleName, "Organization Manager", StringComparison.OrdinalIgnoreCase))
        {
            if (user.OrganizationID.HasValue && outlet.OrganizationID == user.OrganizationID.Value)
            {
                isAuthorized = true;
            }
        }
        else if (string.Equals(user.Role?.RoleName, "Outlet Manager", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(user.Role?.RoleName, "Purchase Manager", StringComparison.OrdinalIgnoreCase))
        {
            if (user.OutletID == request.OutletID)
            {
                isAuthorized = true;
            }
        }
        else if (string.Equals(user.Role?.RoleName, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            isAuthorized = true;
        }

        if (!isAuthorized)
        {
            throw new UnauthorizedAccessException("You are not authorized to submit reviews for this transaction.");
        }

        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderID);
        if (purchaseOrder == null)
            throw new InvalidOperationException("Purchase order does not exist.");

        if (purchaseOrder.OutletID != request.OutletID)
            throw new InvalidOperationException("Purchase order does not belong to this outlet.");

        if (purchaseOrder.VendorID != request.VendorID)
            throw new InvalidOperationException("Purchase order does not belong to this vendor.");

        if (!string.Equals(purchaseOrder.Status, "Delivered", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Feedback can only be submitted after the purchase order has been delivered.");
        }

        var purchaseOrderItem = purchaseOrder.Items?.FirstOrDefault(item => item.POItemID == request.POItemID);
        if (purchaseOrderItem == null)
            throw new InvalidOperationException("Purchase order item does not belong to this purchase order.");

        var alreadyExists = await _feedbackRepository.ExistsForPOItemAsync(
            request.PurchaseOrderID,
            request.POItemID);

        if (alreadyExists)
            throw new InvalidOperationException("Feedback has already been submitted for this purchase order item.");

        var feedback = new VendorFeedbackEntity
        {
            VendorID = request.VendorID,
            OutletID = request.OutletID,
            PurchaseOrderID = request.PurchaseOrderID,
            POItemID = request.POItemID,
            RatedByUserID = ratedByUserId,
            Rating = request.Rating,
            ProductQualityRating = request.ProductQualityRating,
            DeliveryRating = request.DeliveryRating,
            Review = request.Review.Trim(),
            FeedbackDate = DateTime.Now
        };

        var createdFeedback = await _feedbackRepository.AddAsync(feedback);

        // Send notification to Vendor Manager
        try
        {
            var allUsers = await _userRepository.GetAllAsync();
            var vendorUsers = allUsers.Where(u => u.VendorID == request.VendorID).ToList();
            foreach (var vu in vendorUsers)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserID = vu.UserID,
                    Title = "New Vendor Review",
                    Message = $"A review ({feedback.Rating:0.0}/5 stars) has been submitted for PO #{purchaseOrder.PurchaseOrderID}.",
                    NotificationType = "VendorReview",
                    RelatedRequestID = createdFeedback.FeedbackID,
                    RelatedVendorID = request.VendorID,
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Review Notification Error]: {ex.Message}");
        }

        return new CreateVendorFeedbackResponse
        {
            Feedback = MapToDto(createdFeedback)
        };
    }

    private static VendorFeedbackDto MapToDto(VendorFeedbackEntity feedback)
    {
        return new VendorFeedbackDto
        {
            FeedbackID = feedback.FeedbackID,
            VendorID = feedback.VendorID,
            OutletID = feedback.OutletID,
            PurchaseOrderID = feedback.PurchaseOrderID,
            POItemID = feedback.POItemID,
            RatedByUserID = feedback.RatedByUserID,
            Rating = feedback.Rating,
            ProductQualityRating = feedback.ProductQualityRating,
            DeliveryRating = feedback.DeliveryRating,
            Review = feedback.Review,
            FeedbackDate = feedback.FeedbackDate
        };
    }
}

