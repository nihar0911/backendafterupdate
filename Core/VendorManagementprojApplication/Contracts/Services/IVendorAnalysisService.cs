using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Contracts.Services;

public interface IVendorAnalysisService
{
    Task<VendorAnalysisDto> AnalyzeVendorAsync(int vendorID,int productID,int outletID);
}