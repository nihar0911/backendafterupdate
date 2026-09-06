using System;

namespace VendorManagementprojDomain.Entities;

public class Notification
{
    public int NotificationID { get; set; }

    public int UserID { get; set; }

    public int? RelatedRequestID { get; set; }

    public int? RelatedVendorID { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string NotificationType { get; set; } = "General"; // ProcurementOpportunity, OpportunityRejected, General

    public bool IsRead { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
