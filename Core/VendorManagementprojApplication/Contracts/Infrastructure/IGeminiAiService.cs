using System.Collections.Generic;
using System.Threading.Tasks;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Infrastructure;

public interface IGeminiAiService
{
    Task<VendorAiInsightsDto> GenerateVendorInsightsAsync(
        string vendorName,
        decimal avgRating,
        decimal avgQuality,
        decimal avgDelivery,
        List<VendorFeedback> reviews);
}
