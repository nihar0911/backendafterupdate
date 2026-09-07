using System.Collections.Generic;
using System.Threading.Tasks;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Contracts.Infrastructure;

public interface IGeminiAiService
{
    Task<SpoilageAdvisorAiResultDto> GenerateSpoilageAdviceAsync(
        string vendorName,
        int purchaseOrderID,
        bool isSingleProduct,
        List<ProductSpoilageAdviceDto> products);
}

