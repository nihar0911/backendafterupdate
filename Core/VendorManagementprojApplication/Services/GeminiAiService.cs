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
using VendorManagementprojDomain.Entities;

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

    public async Task<VendorAiInsightsDto> GenerateVendorInsightsAsync(
        string vendorName,
        decimal avgRating,
        decimal avgQuality,
        decimal avgDelivery,
        List<VendorFeedback> reviews)
    {
        if (reviews == null || reviews.Count == 0)
        {
            return new VendorAiInsightsDto
            {
                OverallSentiment = "Neutral",
                SentimentScore = 70,
                KeyStrengths = new List<string> { "Active supplier catalog", "Available for purchase requests" },
                RiskFlags = new List<string> { "No verified review history recorded in database yet" },
                ExecutiveSummary = $"{vendorName} does not have any recorded customer reviews yet. Orders will establish baseline performance metrics.",
                TotalReviewsAnalyzed = 0
            };
        }

        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            try
            {
                var reviewsText = new StringBuilder();
                foreach (var r in reviews.Take(15))
                {
                    reviewsText.AppendLine($"- Outlet: {r.Outlet?.OutletName}, Product: {r.POItem?.Product?.ProductName}, Overall: {r.Rating}/5, Quality: {r.ProductQualityRating}/5, Delivery: {r.DeliveryRating}/5, Comment: \"{r.Review}\"");
                }

                string prompt = $@"
You are an expert AI Procurement and Quality Auditor.
Analyze the following verified database reviews and performance records for vendor '{vendorName}':
- Overall Rating: {avgRating:0.0} / 5.0
- Quality Rating: {avgQuality:0.0} / 5.0
- Delivery Rating: {avgDelivery:0.0} / 5.0

Customer Review Logs:
{reviewsText}

Return a STRICT JSON object ONLY (no markdown formatting, no code blocks, no other text) with the following structure:
{{
  ""overallSentiment"": ""Positive"" | ""Mixed"" | ""Needs Attention"",
  ""sentimentScore"": integer between 0 and 100,
  ""keyStrengths"": [""point 1"", ""point 2""],
  ""riskFlags"": [""watch-out 1""],
  ""executiveSummary"": ""A concise 2-sentence procurement recommendation summarizing overall product quality, delivery consistency, and kitchen reliability.""
}}";

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
                        maxOutputTokens = 500
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
                        rawText = rawText.Replace("`json", "").Replace("`", "").Trim();

                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var parsed = JsonSerializer.Deserialize<VendorAiInsightsDto>(rawText, options);
                        if (parsed != null)
                        {
                            parsed.TotalReviewsAnalyzed = reviews.Count;
                            return parsed;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GeminiAiService] Gemini API call error: {ex.Message}");
            }
        }

        // Reliable fallback if Gemini key is not set or network fails
        int positiveCount = reviews.Count(r => r.Rating >= 4.0m);
        int sentimentPct = (int)Math.Round((decimal)positiveCount / Math.Max(1, reviews.Count) * 100m);

        var strengths = new List<string>();
        if (avgQuality >= 4.0m) strengths.Add($"Consistently high product quality standard ({avgQuality:0.0}/5.0?)");
        if (avgDelivery >= 4.0m) strengths.Add($"Dependable delivery turnaround ({avgDelivery:0.0}/5.0?)");
        if (strengths.Count == 0) strengths.Add("Active supplier with verified delivery track record");

        var risks = new List<string>();
        if (avgDelivery < 4.0m) risks.Add("Minor delivery turnaround delays noted in review history");
        if (risks.Count == 0) risks.Add("No major operational risks flagged");

        return new VendorAiInsightsDto
        {
            OverallSentiment = sentimentPct >= 75 ? "Positive" : (sentimentPct >= 50 ? "Mixed" : "Needs Attention"),
            SentimentScore = sentimentPct,
            KeyStrengths = strengths,
            RiskFlags = risks,
            ExecutiveSummary = $"{vendorName} maintains a {avgRating:0.0}/5 customer rating across {reviews.Count} verified deliveries. High reliability for kitchen procurement.",
            TotalReviewsAnalyzed = reviews.Count
        };
    }
}
