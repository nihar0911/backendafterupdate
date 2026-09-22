using System;

namespace VendorManagementprojDomain.Entities;

public class VendorOpportunityResponse
{
    public int ResponseID { get; set; }

    public int RequestID { get; set; }

    public int? RequestItemID { get; set; }

    public int VendorID { get; set; }

    public int ProductID { get; set; }

    public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected

    public string? RejectionReason { get; set; }

    public DateTime? ResponseDate { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public PurchaseRequest? PurchaseRequest { get; set; }

    public PurchaseRequestItem? RequestItem { get; set; }

    public Vendor? Vendor { get; set; }

    public Product? Product { get; set; }
}
