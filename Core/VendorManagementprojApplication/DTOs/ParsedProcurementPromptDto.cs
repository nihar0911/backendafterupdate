using System.Collections.Generic;

namespace VendorManagementprojApplication.DTOs;

public class ParsedProcurementPromptDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ExtractedOutletName { get; set; }
    public string? ParsedRequiredDate { get; set; }
    public List<ExtractedProcurementItemDto> ExtractedItems { get; set; } = new();
}

public class ExtractedProcurementItemDto
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
}
