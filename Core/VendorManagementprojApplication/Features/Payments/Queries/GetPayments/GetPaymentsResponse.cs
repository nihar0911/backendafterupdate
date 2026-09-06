using System.Collections.Generic;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Payments.Queries.GetPayments;

public class GetPaymentsResponse
{
    public List<PaymentDto> Payments { get; set; } = new();
}