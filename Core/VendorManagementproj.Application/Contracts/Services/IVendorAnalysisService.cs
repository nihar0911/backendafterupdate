using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Contracts.Services;

public interface IVendorAnalysisService
{
    Task<VendorAnalysisDto> AnalyzeVendorAsync(int vendorID,int productID,int outletID);
}