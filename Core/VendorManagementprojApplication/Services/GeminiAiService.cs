using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VendorManagementprojApplication.Contracts.Infrastructure;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Services;

public class GeminiAiService : IGeminiAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiAiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        var cfgKey = configuration["GeminiSettings:ApiKey"];
        if (string.IsNullOrWhiteSpace(cfgKey))
        {
            cfgKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? Environment.GetEnvironmentVariable("GeminiSettings__ApiKey") ?? string.Empty;
        }
        _apiKey = cfgKey;
        _model = configuration["GeminiSettings:Model"] ?? "gemini-1.5-flash";
    }

    public async Task<SpoilageAdvisorAiResultDto> GenerateSpoilageAdviceAsync(
        string vendorName,
        int purchaseOrderID,
        bool isSingleProduct,
        List<ProductSpoilageAdviceDto> products)
    {
        if (products == null || products.Count == 0)
        {
            return new SpoilageAdvisorAiResultDto
            {
                OverallSummary = "No products to analyze.",
                ProductNarratives = new List<SpoilageAiNarrativeDto>()
            };
        }

        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            try
            {
                string prompt = BuildPrompt(vendorName, purchaseOrderID, isSingleProduct, products);

                string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    },
                    generationConfig = new
                    {
                        temperature = 0.2,
                        maxOutputTokens = 800
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonResponse);

                    if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                        candidates.GetArrayLength() > 0)
                    {
                        var textElement = candidates[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text");

                        string rawText = textElement.GetString() ?? string.Empty;
                        rawText = rawText.Replace("```json", "").Replace("```", "").Trim();

                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var parsedResponse = JsonSerializer.Deserialize<GeminiAdvisorResponse>(rawText, options);

                        if (parsedResponse != null && parsedResponse.Products != null && parsedResponse.Products.Count > 0)
                        {
                            var result = new SpoilageAdvisorAiResultDto
                            {
                                OverallSummary = !string.IsNullOrWhiteSpace(parsedResponse.OverallSummary)
                                    ? parsedResponse.OverallSummary
                                    : BuildFallbackOverallSummary(isSingleProduct, products),
                                ProductNarratives = new List<SpoilageAiNarrativeDto>()
                            };

                            foreach (var p in products)
                            {
                                var match = parsedResponse.Products.FirstOrDefault(x => x.ProductID == p.ProductID)
                                            ?? parsedResponse.Products.FirstOrDefault(x => string.Equals(x.ProductName, p.ProductName, StringComparison.OrdinalIgnoreCase));

                                if (match != null && !string.IsNullOrWhiteSpace(match.Why) && !string.IsNullOrWhiteSpace(match.RecommendedAction))
                                {
                                    result.ProductNarratives.Add(new SpoilageAiNarrativeDto
                                    {
                                        ProductID = p.ProductID,
                                        Why = match.Why,
                                        RecommendedAction = match.RecommendedAction
                                    });
                                }
                                else
                                {
                                    var fallback = GenerateFallbackForProduct(p, isSingleProduct);
                                    result.ProductNarratives.Add(fallback);
                                }
                            }

                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GeminiAiService] Spoilage Advice Gemini API call error: {ex.Message}");
            }
        }

        // Deterministic fallback
        return GenerateDeterministicFallback(isSingleProduct, products);
    }

    private static string BuildPrompt(
        string vendorName,
        int purchaseOrderID,
        bool isSingleProduct,
        List<ProductSpoilageAdviceDto> products)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are an expert AI Food Logistics and Spoilage Prevention Advisor.");
        sb.AppendLine($"Analyze the following verified factual metrics for Purchase Order #{purchaseOrderID} from vendor '{vendorName}':");
        sb.AppendLine();

        if (isSingleProduct)
        {
            var p = products[0];
            string unitStr = !string.IsNullOrWhiteSpace(p.Unit) ? p.Unit : "units";
            sb.AppendLine("ORDER TYPE: Single Product Order");
            sb.AppendLine($"- Product: {p.ProductName} (ProductID: {p.ProductID})");
            sb.AppendLine($"- Unit: {unitStr}");
            sb.AppendLine($"- Current PO Batch Quantity: {p.CurrentQuantity:0.##} {unitStr}");
            sb.AppendLine($"- Risk Level: {p.RiskLevel}");
            sb.AppendLine($"- Previous Confirmed Deliveries Analyzed: {p.DeliveryCount}");
            sb.AppendLine($"- Total Ordered Historically: {p.TotalOrderedQuantity:0.##} {unitStr}");
            sb.AppendLine($"- Total Received Historically: {p.TotalReceivedQuantity:0.##} {unitStr}");
            sb.AppendLine($"- Total Spoiled Historically: {p.TotalSpoiledQuantity:0.##} {unitStr}");
            sb.AppendLine($"- Historical Volume-Weighted Spoilage: {(p.WeightedSpoilagePercentage.HasValue ? p.WeightedSpoilagePercentage.Value.ToString("0.0") + "%" : "N/A")}");
            sb.AppendLine($"- Recent Spoilage Rate (Recent deliveries): {(p.RecentSpoilagePercentage.HasValue ? p.RecentSpoilagePercentage.Value.ToString("0.0") + "%" : "N/A")}");
            sb.AppendLine($"- Recent Spoilage Trend: {p.Trend}");
            sb.AppendLine();
            sb.AppendLine("Instructions for Single Product Order:");
            sb.AppendLine("1. In 'why', provide a factual quantitative explanation explicitly stating: the number of previous confirmed deliveries analyzed, total spoiled quantity out of total received quantity, historical spoilage rate, recent spoilage rate, recent trend, and risk level.");
            sb.AppendLine("2. In 'recommendedAction', provide a clear, specific dispatch priority recommendation for the vendor manager grounded in these numbers.");
            sb.AppendLine("3. In 'overallSummary', provide a concise 1-sentence overall dispatch assessment for this product.");
            sb.AppendLine("4. Do NOT use ranking words (FIRST / SECOND / THIRD) for single product orders.");
        }
        else
        {
            sb.AppendLine($"ORDER TYPE: Multi-Product Order ({products.Count} Products)");
            sb.AppendLine("The products have already been deterministically ranked by the backend in authoritative priority order:");
            sb.AppendLine();

            foreach (var p in products)
            {
                string unitStr = !string.IsNullOrWhiteSpace(p.Unit) ? p.Unit : "units";
                sb.AppendLine($"[Priority {p.PriorityRank}] Product: {p.ProductName} (ProductID: {p.ProductID})");
                sb.AppendLine($"  - Current PO Quantity: {p.CurrentQuantity:0.##} {unitStr}");
                sb.AppendLine($"  - Risk Level: {p.RiskLevel}");
                sb.AppendLine($"  - Confirmed Deliveries: {p.DeliveryCount}");
                sb.AppendLine($"  - Historical Received: {p.TotalReceivedQuantity:0.##} {unitStr}, Spoiled: {p.TotalSpoiledQuantity:0.##} {unitStr}");
                sb.AppendLine($"  - Historical Spoilage: {(p.WeightedSpoilagePercentage.HasValue ? p.WeightedSpoilagePercentage.Value.ToString("0.0") + "%" : "N/A")}");
                sb.AppendLine($"  - Recent Spoilage: {(p.RecentSpoilagePercentage.HasValue ? p.RecentSpoilagePercentage.Value.ToString("0.0") + "%" : "N/A")}");
                sb.AppendLine($"  - Trend: {p.Trend}");
                sb.AppendLine();
            }

            sb.AppendLine("Instructions for Multi-Product Order:");
            sb.AppendLine("1. The backend PriorityRank is AUTHORITATIVE. Do NOT re-rank or alter the product order.");
            sb.AppendLine("2. For each product's 'why', explicitly identify its dispatch sequence position ('FIRST' for Priority 1, 'SECOND' for Priority 2, 'THIRD' for Priority 3, etc.) and explain WHY it has that priority using its risk level, historical spoilage %, recent rate, trend, and delivery metrics.");
            sb.AppendLine("3. For each product's 'recommendedAction', provide a concise dispatch action reflecting its dispatch priority position.");
            sb.AppendLine("4. In 'overallSummary', provide a clear multi-product dispatch recommendation stating which product to dispatch FIRST, SECOND, THIRD, etc.");
        }

        sb.AppendLine();
        sb.AppendLine("STRICT RESTRICTIONS FOR ALL ORDERS:");
        sb.AppendLine("- Do NOT invent, assume, or mention storage temperature, refrigeration, weather, humidity, transit delays, packaging, shelf life, warehouse conditions, or causes not present in the metrics.");
        sb.AppendLine("- Ground all statements strictly in the supplied delivery numbers, quantities, spoilage percentages, trend, and risk levels.");
        sb.AppendLine("- Keep text concise, professional, and business-oriented.");
        sb.AppendLine();
        sb.AppendLine("Return a STRICT JSON object ONLY (no markdown formatting, no code blocks, no other text) with the following structure:");
        sb.AppendLine("{");
        sb.AppendLine("  \"overallSummary\": \"Concise overall dispatch summary.\",");
        sb.AppendLine("  \"products\": [");
        sb.AppendLine("    {");
        sb.AppendLine("      \"productId\": 123,");
        sb.AppendLine("      \"why\": \"Factual quantitative explanation.\",");
        sb.AppendLine("      \"recommendedAction\": \"Specific actionable dispatch recommendation.\"");
        sb.AppendLine("    }");
        sb.AppendLine("  ]");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static SpoilageAdvisorAiResultDto GenerateDeterministicFallback(
        bool isSingleProduct,
        List<ProductSpoilageAdviceDto> products)
    {
        var result = new SpoilageAdvisorAiResultDto
        {
            OverallSummary = BuildFallbackOverallSummary(isSingleProduct, products),
            ProductNarratives = products.Select(p => GenerateFallbackForProduct(p, isSingleProduct)).ToList()
        };

        return result;
    }

    private static string BuildFallbackOverallSummary(bool isSingleProduct, List<ProductSpoilageAdviceDto> products)
    {
        if (products.Count == 0)
            return "No products evaluated.";

        if (isSingleProduct)
        {
            var single = products[0];
            decimal histVal = single.WeightedSpoilagePercentage ?? 0m;
            return single.DeliveryCount == 0 || string.Equals(single.RiskLevel, "INSUFFICIENT DATA", StringComparison.OrdinalIgnoreCase)
                ? $"Spoilage advice for {single.ProductName}: No previous confirmed delivery history available."
                : $"Spoilage risk for {single.ProductName} is {single.RiskLevel} based on {single.DeliveryCount} previous confirmed deliveries with {histVal:0.0}% historical spoilage.";
        }

        var topPriority = products.FirstOrDefault(p => p.PriorityRank == 1) ?? products[0];
        var sequenceStr = string.Join(", ", products.Select(p => $"{p.ProductName} (Priority {p.PriorityRank})"));
        return $"{products.Count} products evaluated. Dispatch sequence: {sequenceStr}. Prioritize '{topPriority.ProductName}' for dispatch FIRST.";
    }

    private static SpoilageAiNarrativeDto GenerateFallbackForProduct(ProductSpoilageAdviceDto p, bool isSingleProduct)
    {
        string unitStr = !string.IsNullOrWhiteSpace(p.Unit) ? p.Unit : "units";
        decimal histVal = p.WeightedSpoilagePercentage ?? 0m;
        decimal recVal = p.RecentSpoilagePercentage ?? histVal;

        if (p.DeliveryCount == 0 || string.Equals(p.RiskLevel, "INSUFFICIENT DATA", StringComparison.OrdinalIgnoreCase))
        {
            return new SpoilageAiNarrativeDto
            {
                ProductID = p.ProductID,
                Why = isSingleProduct
                    ? $"No confirmed delivery history is available for {p.ProductName} from this vendor."
                    : $"{p.ProductName} should be dispatched {GetOrdinalWord(p.PriorityRank ?? 1)} (Priority {p.PriorityRank}) with no previous confirmed delivery history.",
                RecommendedAction = isSingleProduct
                    ? "Monitor the first confirmed delivery and record actual spoilage for future analysis."
                    : $"Dispatch {p.ProductName} in sequence and monitor initial delivery records."
            };
        }

        if (isSingleProduct)
        {
            if (string.Equals(p.RiskLevel, "HIGH", StringComparison.OrdinalIgnoreCase))
            {
                return new SpoilageAiNarrativeDto
                {
                    ProductID = p.ProductID,
                    Why = $"{p.ProductName} has {p.DeliveryCount} previous confirmed deliveries with {p.TotalSpoiledQuantity:0.##} {unitStr} spoiled from {p.TotalReceivedQuantity:0.##} {unitStr} received, resulting in a historical spoilage rate of {histVal:0.0}%. Recent spoilage is {recVal:0.0}% and the trend is {p.Trend}, indicating HIGH historical spoilage risk.",
                    RecommendedAction = $"Prioritize {p.ProductName} for dispatch because its historical spoilage rate is elevated and recent spoilage is {p.Trend.ToLowerInvariant()}."
                };
            }

            if (string.Equals(p.RiskLevel, "MEDIUM", StringComparison.OrdinalIgnoreCase))
            {
                return new SpoilageAiNarrativeDto
                {
                    ProductID = p.ProductID,
                    Why = $"{p.ProductName} has {p.DeliveryCount} previous confirmed deliveries with {p.TotalSpoiledQuantity:0.##} {unitStr} spoiled from {p.TotalReceivedQuantity:0.##} {unitStr} received, resulting in a historical spoilage rate of {histVal:0.0}%. Recent spoilage is {recVal:0.0}% and the trend is {p.Trend}.",
                    RecommendedAction = $"Maintain {p.ProductName}'s calculated dispatch priority based on the current risk assessment."
                };
            }

            // LOW
            return new SpoilageAiNarrativeDto
            {
                ProductID = p.ProductID,
                Why = $"{p.ProductName} has {p.DeliveryCount} previous confirmed deliveries with {p.TotalSpoiledQuantity:0.##} {unitStr} spoiled from {p.TotalReceivedQuantity:0.##} {unitStr} received, with a low historical spoilage rate of {histVal:0.0}% and a {p.Trend} trend.",
                RecommendedAction = $"Follow the calculated dispatch sequence; historical spoilage risk for {p.ProductName} is low."
            };
        }

        // Multi-Product Fallback
        string ordinal = GetOrdinalWord(p.PriorityRank ?? 1);
        string whyText = $"{p.ProductName} should be dispatched {ordinal} because it is Priority {p.PriorityRank} with {p.RiskLevel} risk, {histVal:0.0}% historical spoilage across {p.DeliveryCount} confirmed deliveries ({p.TotalSpoiledQuantity:0.##} {unitStr} spoiled from {p.TotalReceivedQuantity:0.##} {unitStr} received), and a {p.Trend} trend.";
        string actionText = p.PriorityRank == 1
            ? $"Prioritize {p.ProductName} as the first product to dispatch."
            : $"Dispatch {p.ProductName} {ordinal.ToLowerInvariant()} following higher-priority items.";

        return new SpoilageAiNarrativeDto
        {
            ProductID = p.ProductID,
            Why = whyText,
            RecommendedAction = actionText
        };
    }

    private static string GetOrdinalWord(int rank)
    {
        return rank switch
        {
            1 => "FIRST",
            2 => "SECOND",
            3 => "THIRD",
            4 => "FOURTH",
            5 => "FIFTH",
            6 => "SIXTH",
            7 => "SEVENTH",
            8 => "EIGHTH",
            9 => "NINTH",
            10 => "TENTH",
            _ => $"#{rank}"
        };
    }

    private class GeminiAdvisorResponse
    {
        public string OverallSummary { get; set; } = string.Empty;
        public List<GeminiProductNarrativeItem>? Products { get; set; }
    }

    private class GeminiProductNarrativeItem
    {
        public int ProductID { get; set; }
        public string? ProductName { get; set; }
        public string Why { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
    }
}



