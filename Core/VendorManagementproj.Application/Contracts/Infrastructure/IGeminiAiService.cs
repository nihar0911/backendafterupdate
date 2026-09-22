using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Contracts.Infrastructure;

public interface IGeminiAiService
{
    Task<SpoilageAdvisorAiResultDto> GenerateSpoilageAdviceAsync(
        string vendorName,
        int purchaseOrderID,
        bool isSingleProduct,
        List<ProductSpoilageAdviceDto> products);

    Task<ParsedProcurementPromptDto> ParseProcurementPromptAsync(
        string prompt,
        DateTime referenceDate);

    Task<List<string>> MatchSpokenPhraseToCatalogAsync(
        string spokenPhrase,
        List<string> catalogProductNames);
}
