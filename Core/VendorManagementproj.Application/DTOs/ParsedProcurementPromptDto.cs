using System.Collections.Generic;

namespace VendorManagementprojApplication.DTOs;

public class ParsedProcurementPromptDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ExtractedOutletName { get; set; }
    public string? ParsedRequiredDate { get; set; }
    public string? DetectedLanguage { get; set; } // "hi" or "en"
    public string? AssistantMessage { get; set; }
    public List<ExtractedProcurementItemDto> ExtractedItems { get; set; } = new();
}

public class ExtractedProcurementItemDto
{
    public string ProductName { get; set; } = string.Empty; // Raw spoken phrase (e.g. "शिमला मिर्च", "ताजा चिकन", "tomatoes")
    public string? NormalizedName { get; set; } // Standard English/commercial name (e.g. "Capsicum", "Fresh Chicken", "Tomatoes")
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
}
