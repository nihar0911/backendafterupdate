using System.Collections.Generic;

namespace VendorManagementproj.Application.DTOs;

public class ParsedProcurementItemDto
{
    public int? ProductID { get; set; }
    public string? ProductName { get; set; }
    public string SpokenProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string ResolutionStatus { get; set; } = string.Empty;
    public string? Message { get; set; }
    public List<string>? AmbiguousMatches { get; set; }
}
