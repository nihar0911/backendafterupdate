using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Infrastructure;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Queries.ParseVoiceProcurementOrder;

public class ParseVoiceProcurementOrderQueryHandler
    : IRequestHandler<ParseVoiceProcurementOrderQuery, ParseVoiceProcurementOrderResponse>
{
    private readonly IGeminiAiService _geminiAiService;
    private readonly IProductRepository _productRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public ParseVoiceProcurementOrderQueryHandler(
        IGeminiAiService geminiAiService,
        IProductRepository productRepository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService)
    {
        _geminiAiService = geminiAiService;
        _productRepository = productRepository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<ParseVoiceProcurementOrderResponse> Handle(
        ParseVoiceProcurementOrderQuery request,
        CancellationToken cancellationToken)
    {
        // STEP 1 — Validate prompt
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            return new ParseVoiceProcurementOrderResponse
            {
                Success = false,
                Message = "Procurement prompt cannot be empty.",
                OutletResolutionStatus = "NotSpecified",
                DateResolutionStatus = "NotSpecified",
                Items = new List<ParsedProcurementItemDto>()
            };
        }

        // STEP 2 — Call Gemini / NLP extraction service
        DateTime referenceDate = DateTime.Now;
        var extraction = await _geminiAiService.ParseProcurementPromptAsync(request.Prompt, referenceDate);

        if (extraction == null || !extraction.Success)
        {
            return new ParseVoiceProcurementOrderResponse
            {
                Success = false,
                Message = extraction?.Message ?? "Failed to parse procurement details from prompt.",
                OutletResolutionStatus = "Unresolved",
                DateResolutionStatus = "Unresolved",
                Items = new List<ParsedProcurementItemDto>()
            };
        }

        // STEP 3 — Resolve Outlet against authoritative database
        var allOutlets = await _outletRepository.GetAllAsync();
        int? outletId = null;
        string? outletName = null;
        string outletStatus;

        string? extractedOutlet = extraction.ExtractedOutletName?.Trim();

        if (string.IsNullOrWhiteSpace(extractedOutlet))
        {
            // Outlet not specified in prompt
            if (_currentUserService.IsPurchaseManager && _currentUserService.OutletID.HasValue)
            {
                var assignedOutlet = allOutlets.FirstOrDefault(o => o.OutletID == _currentUserService.OutletID.Value);
                if (assignedOutlet != null)
                {
                    outletId = assignedOutlet.OutletID;
                    outletName = assignedOutlet.OutletName;
                    outletStatus = "Resolved";
                }
                else
                {
                    outletStatus = "NotSpecified";
                }
            }
            else
            {
                outletStatus = "NotSpecified";
            }
        }
        else
        {
            var matchedOutlets = FindMatchingOutlets(extractedOutlet, allOutlets);

            if (matchedOutlets.Count == 0)
            {
                outletStatus = "NotFound";
            }
            else if (matchedOutlets.Count > 1)
            {
                outletStatus = "Ambiguous";
            }
            else
            {
                var targetOutlet = matchedOutlets[0];

                // Check authorization/scoping
                bool isAuthorized = false;
                if (_currentUserService.IsAdmin)
                {
                    isAuthorized = true;
                }
                else if (_currentUserService.IsPurchaseManager)
                {
                    if (_currentUserService.OutletID.HasValue && targetOutlet.OutletID == _currentUserService.OutletID.Value)
                    {
                        isAuthorized = true;
                    }
                    else if (!_currentUserService.OutletID.HasValue && _currentUserService.OrganizationID.HasValue && targetOutlet.OrganizationID == _currentUserService.OrganizationID.Value)
                    {
                        isAuthorized = true;
                    }
                }

                if (isAuthorized)
                {
                    outletId = targetOutlet.OutletID;
                    outletName = targetOutlet.OutletName;
                    outletStatus = "Resolved";
                }
                else
                {
                    outletStatus = "Unauthorized";
                }
            }
        }

        // STEP 4 & 5 & 6 — Resolve Products, Quantities, and Units
        string detectedLanguage = extraction.DetectedLanguage ?? "en";
        var allProducts = await _productRepository.GetAllAsync();
        var activeProducts = allProducts
            .Where(p => string.Equals(p.Status, "Active", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var resolvedItems = new List<ParsedProcurementItemDto>();

        foreach (var extractedItem in extraction.ExtractedItems)
        {
            var itemDto = new ParsedProcurementItemDto
            {
                SpokenProductName = extractedItem.ProductName,
                Quantity = extractedItem.Quantity,
                Unit = extractedItem.Unit
            };

            // Validate Quantity
            if (extractedItem.Quantity <= 0)
            {
                itemDto.ResolutionStatus = "InvalidQuantity";
                itemDto.Message = detectedLanguage == "hi" 
                    ? "कृपया मात्रा बताएं।" 
                    : "Quantity must be greater than zero.";
                resolvedItems.Add(itemDto);
                continue;
            }

            // Resolve Product against active database products dynamically
            var candidateProducts = await FindCandidateProductsAsync(
                extractedItem.ProductName,
                extractedItem.NormalizedName,
                activeProducts);

            if (candidateProducts.Count == 0)
            {
                itemDto.ResolutionStatus = "NotFound";
                itemDto.ProductID = null;
                itemDto.ProductName = null;
                itemDto.Message = detectedLanguage == "hi"
                    ? $"यह product ('{extractedItem.ProductName}') अभी product catalog में उपलब्ध नहीं है।"
                    : $"Product '{extractedItem.ProductName}' is not available in the active product catalog.";
            }
            else if (candidateProducts.Count > 1)
            {
                itemDto.ResolutionStatus = "Ambiguous";
                itemDto.ProductID = null;
                itemDto.ProductName = null;
                itemDto.AmbiguousMatches = candidateProducts.Select(p => p.ProductName).Distinct().ToList();
                itemDto.Message = detectedLanguage == "hi"
                    ? $"आपको {string.Join(" या ", itemDto.AmbiguousMatches)} चाहिए?"
                    : $"Did you mean {string.Join(" or ", itemDto.AmbiguousMatches)}?";
            }
            else
            {
                var resolvedProduct = candidateProducts[0];
                itemDto.ResolutionStatus = "Resolved";
                itemDto.ProductID = resolvedProduct.ProductID; // Authoritative DB ProductID
                itemDto.ProductName = resolvedProduct.ProductName; // Authoritative DB ProductName
                itemDto.Unit = resolvedProduct.Unit; // Authoritative DB Unit
                itemDto.Message = detectedLanguage == "hi"
                    ? "उत्पाद सफलतापूर्वक पहचाना गया।"
                    : "Product resolved successfully.";
            }

            resolvedItems.Add(itemDto);
        }

        // STEP 7 — Date Resolution
        DateTime? resolvedDate = null;
        string dateStatus;

        if (string.IsNullOrWhiteSpace(extraction.ParsedRequiredDate))
        {
            dateStatus = "NotSpecified";
        }
        else if (DateTime.TryParse(extraction.ParsedRequiredDate, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
        {
            if (parsedDate.Date < referenceDate.Date)
            {
                dateStatus = "Unresolved";
            }
            else
            {
                dateStatus = "Resolved";
                resolvedDate = parsedDate.Date;
            }
        }
        else
        {
            dateStatus = "Unresolved";
        }

        string assistantText = string.Empty;

        // Formulate conversational assistant response based on resolution outcome
        if (resolvedItems.Any(i => i.ResolutionStatus == "Ambiguous"))
        {
            var amb = resolvedItems.First(i => i.ResolutionStatus == "Ambiguous");
            var choices = amb.AmbiguousMatches ?? new List<string>();
            assistantText = detectedLanguage == "hi"
                ? $"आपको {string.Join(" या ", choices)} चाहिए?"
                : $"Did you mean {string.Join(" or ", choices)}?";
        }
        else if (resolvedItems.Any(i => i.ResolutionStatus == "NotFound"))
        {
            var nf = resolvedItems.First(i => i.ResolutionStatus == "NotFound");
            assistantText = detectedLanguage == "hi"
                ? $"यह product अभी product catalog में उपलब्ध नहीं है।"
                : $"Product '{nf.SpokenProductName}' is not available in the product catalog.";
        }
        else if (resolvedItems.All(i => i.ResolutionStatus == "Resolved"))
        {
            var resolvedNames = resolvedItems.Select(i => $"{i.Quantity} {i.Unit} {i.ProductName}").ToList();
            string joined = string.Join(detectedLanguage == "hi" ? " और " : ", ", resolvedNames);
            assistantText = detectedLanguage == "hi"
                ? $"ठीक है। मैंने {joined} की आवश्यकता समझ ली है।"
                : $"Understood procurement requirement for {joined}.";
        }
        else
        {
            assistantText = extraction.AssistantMessage?.Trim() ?? string.Empty;
        }

        // Response Construction
        return new ParseVoiceProcurementOrderResponse
        {
            Success = true,
            Message = detectedLanguage == "hi" 
                ? "खरीद आदेश का सफलतापूर्वक विश्लेषण किया गया।" 
                : "Procurement order parsed and resolved successfully.",
            DetectedLanguage = detectedLanguage,
            AssistantResponseText = assistantText,
            OutletID = outletId,
            OutletName = outletName,
            OutletResolutionStatus = outletStatus,
            RequiredDate = resolvedDate,
            DateResolutionStatus = dateStatus,
            Items = resolvedItems
        };
    }

    private static List<Outlet> FindMatchingOutlets(string extractedOutlet, List<Outlet> outlets)
    {
        string raw = extractedOutlet.Trim().ToLowerInvariant();
        string normalized = Regex.Replace(raw, @"\b(?:outlet|branch|store|location|आउटलेट|शाखा|स्टोर)\b", "").Trim();

        // 1. Exact match on OutletName
        var exact = outlets.Where(o => string.Equals(o.OutletName.Trim(), extractedOutlet, StringComparison.OrdinalIgnoreCase)).ToList();
        if (exact.Count > 0) return exact;

        // 2. Exact match on normalized OutletName
        if (!string.IsNullOrWhiteSpace(normalized))
        {
            var normMatches = outlets.Where(o =>
            {
                string oNorm = Regex.Replace(o.OutletName.Trim().ToLowerInvariant(), @"\b(?:outlet|branch|store|location)\b", "").Trim();
                return string.Equals(oNorm, normalized, StringComparison.OrdinalIgnoreCase);
            }).ToList();

            if (normMatches.Count > 0) return normMatches;
        }

        // 3. Word token / containment match
        string searchToken = !string.IsNullOrWhiteSpace(normalized) ? normalized : raw;
        var tokenMatches = outlets.Where(o =>
        {
            string oName = o.OutletName.Trim().ToLowerInvariant();
            return Regex.IsMatch(oName, $@"\b{Regex.Escape(searchToken)}\b", RegexOptions.IgnoreCase) ||
                   oName.Contains(searchToken, StringComparison.OrdinalIgnoreCase);
        }).ToList();

        return tokenMatches;
    }

    // Optional fast local alias layer for regression backwards compatibility
    private static readonly Dictionary<string, string[]> BilingualSynonyms = new(StringComparer.OrdinalIgnoreCase)
    {
        // Poultry & Meat
        { "चिकन", new[] { "chicken", "poultry" } },
        { "ताजा चिकन", new[] { "fresh chicken" } },
        { "ताज़ा चिकन", new[] { "fresh chicken" } },
        { "फ्रोजन चिकन", new[] { "frozen chicken" } },
        { "मटन", new[] { "mutton", "lamb", "goat" } },
        { "मछली", new[] { "fish", "seafood" } },
        { "अंडे", new[] { "egg", "eggs" } },
        { "अंडा", new[] { "egg", "eggs" } },
        
        // Vegetables
        { "टमाटर", new[] { "tomato", "tomatoes" } },
        { "ताजा टमाटर", new[] { "fresh tomato", "fresh tomatoes" } },
        { "आलू", new[] { "potato", "potatoes" } },
        { "प्याज", new[] { "onion", "onions" } },
        { "गाजर", new[] { "carrot", "carrots" } },
        { "गोभी", new[] { "cauliflower", "cabbage" } },
        { "पत्ता गोभी", new[] { "cabbage" } },
        { "फूल गोभी", new[] { "cauliflower" } },
        { "शिमला मिर्च", new[] { "capsicum", "bell pepper" } },
        { "मिर्च", new[] { "chilli", "chili", "pepper" } },
        { "धनिया", new[] { "coriander", "cilantro" } },
        { "पालक", new[] { "spinach" } },
        { "मटर", new[] { "peas", "green peas" } },
        { "लहसुन", new[] { "garlic" } },
        { "अदरक", new[] { "ginger" } },
        { "खीरा", new[] { "cucumber" } },
        { "नींबू", new[] { "lemon", "lime" } },
        
        // Fruits
        { "संतरे", new[] { "orange", "oranges" } },
        { "संतरा", new[] { "orange", "oranges" } },
        { "सेब", new[] { "apple", "apples" } },
        { "केला", new[] { "banana", "bananas" } },
        { "केले", new[] { "banana", "bananas" } },
        { "आम", new[] { "mango", "mangoes" } },
        { "अंगूर", new[] { "grape", "grapes" } },
        { "पपीता", new[] { "papaya" } },
        { "तरबूज", new[] { "watermelon" } },
        { "अनानास", new[] { "pineapple" } },

        // Dairy & Grocery
        { "दूध", new[] { "milk", "fresh milk" } },
        { "पनीर", new[] { "paneer", "cottage cheese" } },
        { "दही", new[] { "curd", "yogurt" } },
        { "मक्खन", new[] { "butter" } },
        { "घी", new[] { "ghee" } },
        { "चीज", new[] { "cheese" } },
        { "चावल", new[] { "rice", "basmati rice" } },
        { "आटा", new[] { "flour", "wheat flour", "atta" } },
        { "मैदा", new[] { "refined flour", "maida" } },
        { "दाल", new[] { "lentil", "lentils", "pulses", "dal" } },
        { "चीनी", new[] { "sugar" } },
        { "नमक", new[] { "salt" } },
        { "तेल", new[] { "cooking oil", "oil" } },
        { "ब्रेड", new[] { "bread" } },
        { "चाय", new[] { "tea" } },
        { "कॉफ़ी", new[] { "coffee" } },

        // Hinglish
        { "taaza chicken", new[] { "fresh chicken" } },
        { "fresh chicken", new[] { "fresh chicken" } },
        { "frozen chicken", new[] { "frozen chicken" } },
        { "shimla mirch", new[] { "capsicum", "bell pepper" } },
        { "tamatar", new[] { "tomato", "tomatoes" } },
        { "aloo", new[] { "potato", "potatoes" } },
        { "pyaaz", new[] { "onion", "onions" } },
        { "santre", new[] { "orange", "oranges" } },
        { "santra", new[] { "orange", "oranges" } },
        { "doodh", new[] { "milk" } },
        { "chawal", new[] { "rice" } },
        { "machli", new[] { "fish" } },
        { "anda", new[] { "egg", "eggs" } },
        { "ande", new[] { "egg", "eggs" } },
        { "gajar", new[] { "carrot", "carrots" } },
        { "kela", new[] { "banana", "bananas" } },
        { "kele", new[] { "banana", "bananas" } },
        { "seb", new[] { "apple", "apples" } }
    };

    private async Task<List<Product>> FindCandidateProductsAsync(
        string spokenName,
        string? normalizedName,
        List<Product> activeProducts)
    {
        if (activeProducts == null || activeProducts.Count == 0)
        {
            return new List<Product>();
        }

        string spokenRaw = spokenName.Trim();
        string spokenLower = spokenRaw.ToLowerInvariant();
        string spokenClean = NormalizeString(spokenRaw);
        string spokenRoot = GetWordRoot(spokenClean);

        string? normRaw = normalizedName?.Trim();
        string? normLower = normRaw?.ToLowerInvariant();
        string? normClean = normRaw != null ? NormalizeString(normRaw) : null;
        string? normRoot = normClean != null ? GetWordRoot(normClean) : null;

        // 1. Exact full-name match on spoken term or normalized term
        var exactMatches = activeProducts.Where(p =>
        {
            string pClean = NormalizeString(p.ProductName);
            return string.Equals(pClean, spokenClean, StringComparison.OrdinalIgnoreCase) ||
                   (normClean != null && string.Equals(pClean, normClean, StringComparison.OrdinalIgnoreCase));
        }).ToList();

        if (exactMatches.Count == 1)
        {
            return exactMatches;
        }

        // 2. Expand search terms with local synonyms & roots
        var searchTerms = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            spokenRaw,
            spokenLower,
            spokenClean,
            spokenRoot
        };

        if (!string.IsNullOrWhiteSpace(normRaw))
        {
            searchTerms.Add(normRaw);
            if (normLower != null) searchTerms.Add(normLower);
            if (normClean != null) searchTerms.Add(normClean);
            if (normRoot != null) searchTerms.Add(normRoot);
        }

        // Check fallback bilingual synonyms
        if (BilingualSynonyms.TryGetValue(spokenRaw, out var syns) ||
            BilingualSynonyms.TryGetValue(spokenLower, out syns) ||
            BilingualSynonyms.TryGetValue(spokenClean, out syns))
        {
            foreach (var s in syns)
            {
                searchTerms.Add(s);
                string sClean = NormalizeString(s);
                searchTerms.Add(sClean);
                searchTerms.Add(GetWordRoot(sClean));
            }
        }

        // 3. Exact match against synonym target terms
        foreach (var term in searchTerms)
        {
            if (string.IsNullOrWhiteSpace(term)) continue;
            string tClean = NormalizeString(term);
            var directSynMatch = activeProducts
                .Where(p => string.Equals(NormalizeString(p.ProductName), tClean, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (directSynMatch.Count == 1)
            {
                return directSynMatch;
            }
        }

        // 4. Token / Root / Substring matching against active catalog
        var candidates = new List<Product>();

        foreach (var p in activeProducts)
        {
            string pClean = NormalizeString(p.ProductName);
            string pRoot = GetWordRoot(pClean);

            bool matchesAnyTerm = searchTerms.Any(term =>
            {
                if (string.IsNullOrWhiteSpace(term)) return false;
                string tClean = NormalizeString(term);
                string tRoot = GetWordRoot(tClean);

                if (string.Equals(pClean, tClean, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(pRoot, tRoot, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(pClean, tRoot, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(pRoot, tClean, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                var pWords = pClean.Split(new[] { ' ', '-', '/', ',' }, StringSplitOptions.RemoveEmptyEntries);
                return pWords.Any(w =>
                {
                    string wClean = NormalizeString(w);
                    string wRoot = GetWordRoot(wClean);
                    return string.Equals(wClean, tClean, StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(wRoot, tRoot, StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(wClean, tRoot, StringComparison.OrdinalIgnoreCase) ||
                           (!string.IsNullOrWhiteSpace(tRoot) && tRoot.Length > 2 && wClean.StartsWith(tRoot, StringComparison.OrdinalIgnoreCase));
                });
            });

            if (matchesAnyTerm)
            {
                candidates.Add(p);
            }
        }

        var distinctCandidates = candidates.GroupBy(p => p.ProductID).Select(g => g.First()).ToList();
        if (distinctCandidates.Count > 0)
        {
            return distinctCandidates;
        }

        // 5. Dynamic AI Semantic Catalog Matching (for new products without local aliases)
        try
        {
            var catalogProductNames = activeProducts.Select(p => p.ProductName).Distinct().ToList();
            var aiMatches = await _geminiAiService.MatchSpokenPhraseToCatalogAsync(spokenRaw, catalogProductNames);

            if (aiMatches != null && aiMatches.Count > 0)
            {
                var matchedProducts = activeProducts
                    .Where(p => aiMatches.Any(m => string.Equals(p.ProductName, m, StringComparison.OrdinalIgnoreCase)))
                    .GroupBy(p => p.ProductID)
                    .Select(g => g.First())
                    .ToList();

                if (matchedProducts.Count > 0)
                {
                    return matchedProducts;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ParseVoiceProcurementOrderQueryHandler] Dynamic AI catalog matching error: {ex.Message}");
        }

        return new List<Product>();
    }

    private static string NormalizeString(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var cleaned = Regex.Replace(input.ToLowerInvariant(), @"[^\w\u0900-\u097F\s]", " ");
        return Regex.Replace(cleaned, @"\s+", " ").Trim();
    }

    private static string GetWordRoot(string word)
    {
        string w = NormalizeString(word);
        if (w.EndsWith("oes") && w.Length > 4)
            return w.Substring(0, w.Length - 2); // potatoes -> potato, tomatoes -> tomato
        if (w.EndsWith("ies") && w.Length > 4)
            return w.Substring(0, w.Length - 3) + "y"; // berries -> berry
        if (w.EndsWith("es") && w.Length > 3)
            return w.Substring(0, w.Length - 2); // mangoes -> mango, oranges -> orange
        if (w.EndsWith("s") && !w.EndsWith("ss") && w.Length > 3)
            return w.Substring(0, w.Length - 1); // carrots -> carrot, onions -> onion
        return w;
    }
}
