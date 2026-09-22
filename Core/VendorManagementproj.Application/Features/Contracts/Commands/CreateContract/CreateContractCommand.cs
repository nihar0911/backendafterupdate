using System;
using System.Collections.Generic;
using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Contracts.Commands.CreateContract;

public class CreateContractCommand : IRequest<CreateContractResponse>
{
    public int? QuotationID { get; set; }
    public int OutletID { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;

    // Phase 2 Target Architecture: List of product/vendor assignments
    public List<ContractProductAssignmentDto> Items { get; set; } = new();
    public List<ContractProductAssignmentDto> Assignments { get; set; } = new();

    // Legacy fields preserved for backward compatibility
    public int ProductID { get; set; }
    public decimal TotalQuantity { get; set; }
    public List<CreateContractVendorAllocationDto> Allocations { get; set; } = new();
}
