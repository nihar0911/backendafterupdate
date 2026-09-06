using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Persistence;

public interface IComplaintRepository
{
    Task<Complaint> AddAsync(Complaint complaint);

    Task<Complaint?> GetByIdAsync(int complaintID);

    Task<List<Complaint>> GetAllAsync();

    Task<Complaint?> UpdateAsync(Complaint complaint);
}