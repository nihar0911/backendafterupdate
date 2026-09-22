using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Persistence;

public interface IVendorFeedbackRepository
{
    Task<VendorFeedback> AddAsync(VendorFeedback feedback);

    Task<VendorFeedback?> GetByIdAsync(int feedbackID);

    Task<List<VendorFeedback>> GetAllAsync();

    Task<List<VendorFeedback>> GetByVendorIdAsync(int vendorID, int? organizationID = null, int? productID = null);

    Task<List<VendorFeedback>> GetByOrganizationIdAsync(int organizationID);

    Task<bool> ExistsForPOItemAsync(
        int purchaseOrderID,
        int poItemID);
}
