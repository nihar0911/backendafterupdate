using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementproj.Application.Contracts.Infrastructure;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.Contracts.Services;
using VendorManagementproj.Domain.Entities;

namespace VendorManagementproj.Application.Features.Payments.Queries.GetPaymentReceiptPdf;

public class GetPaymentReceiptPdfQueryHandler : IRequestHandler<GetPaymentReceiptPdfQuery, GetPaymentReceiptPdfResponse>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentDocumentService _paymentDocumentService;

    public GetPaymentReceiptPdfQueryHandler(
        IPaymentRepository paymentRepository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService,
        IPaymentDocumentService paymentDocumentService)
    {
        _paymentRepository = paymentRepository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
        _paymentDocumentService = paymentDocumentService;
    }

    public async Task<GetPaymentReceiptPdfResponse> Handle(
        GetPaymentReceiptPdfQuery request,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(request.PaymentID);

        if (payment == null)
            throw new InvalidOperationException("Payment record not found.");

        int? paymentOutletId = payment.Invoice?.OutletID ?? payment.Invoice?.PurchaseOrder?.OutletID;
        int? paymentOrgId = payment.Invoice?.Outlet?.OrganizationID ?? payment.Invoice?.PurchaseOrder?.Outlet?.OrganizationID;

        // Role authorization check
        if (_currentUserService.IsAdmin)
        {
            // Admin has full global access
        }
        else if (_currentUserService.IsOrganizationManager)
        {
            if (!_currentUserService.OrganizationID.HasValue)
            {
                throw new UnauthorizedAccessException("You are not authorized to view receipts without an assigned organization.");
            }

            if (paymentOrgId.HasValue && paymentOrgId.Value != _currentUserService.OrganizationID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view receipts outside your organization.");
            }
            else if (!paymentOrgId.HasValue && paymentOutletId.HasValue)
            {
                var outlet = await _outletRepository.GetByIdAsync(paymentOutletId.Value);
                if (outlet == null || outlet.OrganizationID != _currentUserService.OrganizationID.Value)
                {
                    throw new UnauthorizedAccessException("You are not authorized to view receipts outside your organization.");
                }
            }
        }
        else if (_currentUserService.IsPurchaseManager || _currentUserService.IsOutletManager)
        {
            if (!_currentUserService.OutletID.HasValue)
            {
                throw new UnauthorizedAccessException("You are not authorized to view receipts without an assigned outlet.");
            }

            if (!paymentOutletId.HasValue || paymentOutletId.Value != _currentUserService.OutletID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view receipts outside your assigned outlet.");
            }
        }
        else if (_currentUserService.IsVendorManager)
        {
            if (!_currentUserService.VendorID.HasValue || payment.Invoice?.VendorID != _currentUserService.VendorID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view receipts outside your vendor account.");
            }
        }
        else
        {
            throw new UnauthorizedAccessException("You are not authorized to access payment receipts.");
        }

        var pdfBytes = await _paymentDocumentService.GeneratePaymentReceiptPdfAsync(payment);

        return new GetPaymentReceiptPdfResponse
        {
            PdfBytes = pdfBytes,
            FileName = $"PaymentReceipt-PAY-{payment.PaymentID}.pdf",
            ContentType = "application/pdf"
        };
    }
}
