using System.Collections.Generic;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Payments.Queries.GetPayments;

public class GetPaymentsResponse
{
    public List<PaymentDto> Payments { get; set; } = new();
}