using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Common;

public static class PurchaseOrderApprover
{
    public const string OrganizationManager = "Organization Manager";
    public const string OutletManager = "Outlet Manager";

    public static string Normalize(string? role)
    {
        return string.Equals(role, OutletManager, StringComparison.OrdinalIgnoreCase)
            ? OutletManager
            : OrganizationManager;
    }

    public static void EnsureCurrentUserCanDecide(
        ICurrentUserService currentUser,
        PurchaseOrder purchaseOrder,
        Outlet? outlet)
    {
        if (currentUser.IsAdmin)
        {
            return;
        }

        var requiredRole = Normalize(purchaseOrder.ApproverRole);

        if (requiredRole == OutletManager)
        {
            if (!currentUser.IsOutletManager)
            {
                throw new UnauthorizedAccessException("Only the Outlet Manager can approve or reject this purchase order.");
            }

            if (!currentUser.OutletID.HasValue || currentUser.OutletID.Value != purchaseOrder.OutletID)
            {
                throw new UnauthorizedAccessException("You can only approve purchase orders for your assigned outlet.");
            }

            return;
        }

        if (!currentUser.IsOrganizationManager)
        {
            throw new UnauthorizedAccessException("Only the Organization Manager can approve or reject this purchase order.");
        }

        if (!currentUser.OrganizationID.HasValue)
        {
            throw new UnauthorizedAccessException("You are not assigned to an organization.");
        }

        if (outlet == null || outlet.OrganizationID != currentUser.OrganizationID.Value)
        {
            throw new UnauthorizedAccessException("You are not authorized to approve purchase orders outside your organization.");
        }
    }
}
