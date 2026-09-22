using System;
using System.Collections.Generic;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.PurchaseRequests.Queries.ParseVoiceProcurementOrder;

public class ParseVoiceProcurementOrderResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public int? OutletID { get; set; }

    public string? OutletName { get; set; }

    public string? OutletResolutionStatus { get; set; }

    public DateTime? RequiredDate { get; set; }

    public string? DateResolutionStatus { get; set; }

    public string? DetectedLanguage { get; set; } // "hi" or "en"

    public string? AssistantResponseText { get; set; }

    public List<ParsedProcurementItemDto> Items { get; set; } = new();

    // Single-item convenience properties
    public int? ProductID => Items.Count > 0 ? Items[0].ProductID : null;

    public string? ProductName => Items.Count > 0 ? Items[0].ProductName : null;

    public decimal? Quantity => Items.Count > 0 ? Items[0].Quantity : null;

    public string? Unit => Items.Count > 0 ? Items[0].Unit : null;
}
