using System;
using System.Collections.Generic;

namespace VendorManagementprojApplication.DTOs;

public class PurchaseRequestDto
{
    public int RequestID { get; set; }

    public int OutletID { get; set; }

    public int CreatedByUserID { get; set; }

    public DateTime RequestDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public List<PurchaseRequestItemDto> Items { get; set; } = new();
}
