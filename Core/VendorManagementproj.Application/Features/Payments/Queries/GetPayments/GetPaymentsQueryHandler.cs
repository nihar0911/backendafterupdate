using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Payments.Queries.GetPayments;

public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, GetPaymentsResponse>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetPaymentsQueryHandler(
        IPaymentRepository paymentRepository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService)
    {
        _paymentRepository = paymentRepository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetPaymentsResponse> Handle(
        GetPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        List<Payment> payments;

        if (_currentUserService.IsOrganizationManager)
        {
            if (_currentUserService.OrganizationID.HasValue)
            {
                payments = await _paymentRepository.GetByOrganizationIdAsync(_currentUserService.OrganizationID.Value);
            }
            else
            {
                payments = new List<Payment>();
            }
        }
        else if (_currentUserService.IsVendorManager)
        {
            if (_currentUserService.VendorID.HasValue)
            {
                payments = await _paymentRepository.GetByVendorIdAsync(_currentUserService.VendorID.Value);
            }
            else
            {
                payments = new List<Payment>();
            }
        }
        else if (_currentUserService.IsPurchaseManager || _currentUserService.IsOutletManager)
        {
            if (_currentUserService.OutletID.HasValue)
            {
                var allPayments = await _paymentRepository.GetAllAsync();
                payments = allPayments.Where(p => p.Invoice != null && p.Invoice.OutletID == _currentUserService.OutletID.Value).ToList();
            }
            else
            {
                payments = new List<Payment>();
            }
        }
        else if (_currentUserService.IsAdmin)
        {
            payments = await _paymentRepository.GetAllAsync();
        }
        else
        {
            payments = new List<Payment>();
        }

        var dtos = payments.Select(p => new PaymentDto
        {
            PaymentID = p.PaymentID,
            InvoiceID = p.InvoiceID,
            PurchaseOrderID = p.Invoice?.PurchaseOrderID ?? 0,
            VendorID = p.Invoice?.VendorID ?? 0,
            VendorName = p.Invoice?.Vendor?.VendorName ?? "Vendor",
            OutletID = p.Invoice?.OutletID ?? 0,
            OutletName = p.Invoice?.Outlet?.OutletName ?? "Outlet",
            OrganizationID = p.Invoice?.PurchaseOrder?.Outlet?.OrganizationID ?? p.Invoice?.Outlet?.OrganizationID ?? 0,
            OrganizationName = p.Invoice?.PurchaseOrder?.Outlet?.Organization?.OrganizationName ?? "Organization",
            Amount = p.Amount,
            PaymentDate = p.PaymentDate,
            PaymentMethod = p.PaymentMethod,
            TransactionReference = p.TransactionReference,
            Status = p.Status,
            InvoiceStatus = p.Invoice?.Status ?? "Paid"
        }).ToList();

        return new GetPaymentsResponse
        {
            Payments = dtos
        };
    }
}