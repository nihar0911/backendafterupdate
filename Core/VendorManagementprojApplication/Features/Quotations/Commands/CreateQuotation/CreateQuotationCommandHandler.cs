using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Quotations.Commands.CreateQuotation;

public class CreateQuotationCommandHandler
    : IRequestHandler<CreateQuotationCommand, CreateQuotationResponse>
{
    private readonly IQuotationRepository _quotationRepository;
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;
    private readonly IVendorProductRepository _vendorProductRepository;
    private readonly ITaxRateRepository _taxRateRepository;
    private readonly IProductRepository _productRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IVendorOpportunityResponseRepository _responseRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateQuotationCommandHandler(
        IQuotationRepository quotationRepository,
        IPurchaseRequestRepository purchaseRequestRepository,
        IVendorProductRepository vendorProductRepository,
        ITaxRateRepository taxRateRepository,
        IProductRepository productRepository,
        IVendorRepository vendorRepository,
        IOutletRepository outletRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository,
        IVendorOpportunityResponseRepository responseRepository,
        ICurrentUserService currentUserService)
    {
        _quotationRepository = quotationRepository;
        _purchaseRequestRepository = purchaseRequestRepository;
        _vendorProductRepository = vendorProductRepository;
        _taxRateRepository = taxRateRepository;
        _productRepository = productRepository;
        _vendorRepository = vendorRepository;
        _outletRepository = outletRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _responseRepository = responseRepository;
        _currentUserService = currentUserService;
    }

    public async Task<CreateQuotationResponse> Handle(
        CreateQuotationCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.IsVendorManager)
        {
            if (!_currentUserService.VendorID.HasValue)
            {
                throw new UnauthorizedAccessException("Your vendor account is not configured. Please contact an administrator.");
            }
            request.VendorID = _currentUserService.VendorID.Value;
        }

        var vendor =
            await _vendorRepository.GetByIdAsync(
                request.VendorID);

        if (vendor == null)
            throw new InvalidOperationException(
                "Vendor does not exist.");

        if (!string.Equals(
                vendor.Status,
                "Active",
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Vendor is not active.");

        var purchaseRequest =
            await _purchaseRequestRepository.GetByIdAsync(
                request.RequestID);

        if (purchaseRequest == null)
            throw new InvalidOperationException(
                "Purchase request does not exist.");

        var purchaseRequestItems =
            await _purchaseRequestRepository
                .GetItemsByRequestIdAsync(
                    request.RequestID);

        if (request.Items == null ||
            request.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "Quotation must contain at least one item.");
        }

        var quotation = new Quotation
        {
            RequestID = request.RequestID,
            VendorID = request.VendorID,
            ValidUntil = request.ValidUntil,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Submitted" : request.Status,
            QuotationItems = new List<QuotationItem>()
        };

        foreach (var itemDto in request.Items)
        {
            if (itemDto.Quantity <= 0)
                throw new InvalidOperationException(
                    "Quantity must be greater than zero.");

            var requestedItem =
                purchaseRequestItems.FirstOrDefault(
                    item => item.ProductID == itemDto.ProductID);

            if (requestedItem == null)
                throw new InvalidOperationException(
                    $"ProductID {itemDto.ProductID} was not requested in this purchase request.");

            if (itemDto.Quantity > requestedItem.Quantity)
                throw new InvalidOperationException(
                    $"Quotation quantity for ProductID {itemDto.ProductID} cannot exceed the requested quantity of {requestedItem.Quantity}.");

            var vendorProduct =
                await _vendorProductRepository
                    .GetByVendorAndProductAsync(
                        request.VendorID,
                        itemDto.ProductID);

            if (vendorProduct == null)
                throw new InvalidOperationException(
                    $"Vendor does not supply ProductID {itemDto.ProductID}.");

            if (!string.Equals(
                    vendorProduct.Status,
                    "Active",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"ProductID {itemDto.ProductID} is not currently active for this vendor.");
            }

            var product =
                await _productRepository
                    .GetByIdAsync(
                        itemDto.ProductID);

            if (product == null)
                throw new InvalidOperationException(
                    $"ProductID {itemDto.ProductID} does not exist.");

            if (!string.Equals(
                    product.Status,
                    "Active",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"ProductID {itemDto.ProductID} is not active.");
            }

            var taxRate =
                await _taxRateRepository
                    .GetByIdAsync(
                        product.TaxRateID);

            if (taxRate == null)
                throw new InvalidOperationException(
                    $"No tax rate is configured for ProductID {itemDto.ProductID}.");

            if (!string.Equals(
                    taxRate.Status,
                    "Active",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"The tax rate assigned to ProductID {itemDto.ProductID} is not active.");
            }

            var unitPrice =
                vendorProduct.UnitPrice;

            var grossAmount =
                unitPrice * itemDto.Quantity;

            var taxableAmount =
                grossAmount;

            var taxAmount =
                taxableAmount *
                (taxRate.Percentage / 100m);

            var totalAmount =
                taxableAmount + taxAmount;

            quotation.QuotationItems.Add(
                new QuotationItem
                {
                    ProductID =
                        itemDto.ProductID,

                    Quantity =
                        itemDto.Quantity,

                    UnitPrice =
                        unitPrice,

                    TaxRate =
                        taxRate.Percentage,

                    TaxAmount =
                        taxAmount,

                    TotalAmount =
                        totalAmount
                });
        }

        var createdQuotation =
            await _quotationRepository
                .AddAsync(
                    quotation);

        // Send notification to the responsible Organization Manager(s)
        try
        {
            var targetOutlet = await _outletRepository.GetByIdAsync(purchaseRequest.OutletID);
            string vendorName = vendor?.VendorName ?? "Vendor";

            var allUsers = await _userRepository.GetAllAsync();
            var orgManagers = allUsers.Where(u =>
                (targetOutlet != null && u.OrganizationID == targetOutlet.OrganizationID) &&
                (string.Equals(u.Role?.RoleName, "Organization Manager", StringComparison.OrdinalIgnoreCase) || u.RoleID == 2))
                .ToList();

            if (orgManagers.Count == 0)
            {
                orgManagers = allUsers.Where(u =>
                    string.Equals(u.Role?.RoleName, "Organization Manager", StringComparison.OrdinalIgnoreCase) || u.RoleID == 2)
                    .ToList();
            }

            var notifiedOrgUserIds = new HashSet<int>();

            foreach (var om in orgManagers)
            {
                if (notifiedOrgUserIds.Add(om.UserID))
                {
                    var notif = new Notification
                    {
                        UserID = om.UserID,
                        RelatedRequestID = purchaseRequest.RequestID,
                        RelatedVendorID = request.VendorID,
                        Title = $"New Quotation for PR-{purchaseRequest.RequestID}",
                        Message = $"New quotation submitted for Purchase Request PR-{purchaseRequest.RequestID} by {vendorName}.",
                        NotificationType = "QuotationSubmitted",
                        IsRead = false,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _notificationRepository.AddAsync(notif);
                }
            }

            // Also ensure the PR creator receives the notification
            if (purchaseRequest.CreatedByUserID > 0 && notifiedOrgUserIds.Add(purchaseRequest.CreatedByUserID))
            {
                var creatorNotif = new Notification
                {
                    UserID = purchaseRequest.CreatedByUserID,
                    RelatedRequestID = purchaseRequest.RequestID,
                    RelatedVendorID = request.VendorID,
                    Title = $"New Quotation for PR-{purchaseRequest.RequestID}",
                    Message = $"New quotation submitted for Purchase Request PR-{purchaseRequest.RequestID} by {vendorName}.",
                    NotificationType = "QuotationSubmitted",
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow
                };
                await _notificationRepository.AddAsync(creatorNotif);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreateQuotation] Error sending quotation notification: {ex.Message}");
        }

        return new CreateQuotationResponse
        {
            Quotation =
                MapToDto(
                    createdQuotation)
        };
    }

    private static QuotationDto MapToDto(
        Quotation quotation)
    {
        return new QuotationDto
        {
            QuotationID =
                quotation.QuotationID,

            RequestID =
                quotation.RequestID,

            VendorID =
                quotation.VendorID,

            ValidUntil =
                quotation.ValidUntil,

            Status =
                quotation.Status,

            Items =
                quotation.QuotationItems
                    .Select(item =>
                        new QuotationItemDto
                        {
                            QuotationItemID =
                                item.QuotationItemID,

                            ProductID =
                                item.ProductID,

                            Quantity =
                                item.Quantity,

                            UnitPrice =
                                item.UnitPrice,

                            TaxRate =
                                item.TaxRate,

                            TaxAmount =
                                item.TaxAmount,

                            TotalAmount =
                                item.TotalAmount
                        })
                    .ToList()
        };
    }
}
