using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Persistence;

public interface IContractRepository
{
    Task<Contract> AddAsync(Contract contract);

    Task<Contract?> GetByIdAsync(int contractID);

    Task<List<Contract>> GetAllAsync();

    Task<Contract?> UpdateAsync(Contract contract);

    Task<bool> DeleteAsync(int contractID);

    Task<List<ContractVendorAllocation>> GetVendorAllocationsAsync(
        int vendorID,
        int productID,
        int outletID);

    Task<List<ContractVendorAllocation>> GetVendorAllocationsForAnalysisAsync(
        int vendorID,
        int productID,
        int outletID);
}