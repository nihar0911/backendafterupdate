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
                itemDto.Message = "Quantity must be greater than zero.";
                resolvedItems.Add(itemDto);
                continue;
            }

            // Resolve Product against active database products
            var candidateProducts = FindCandidateProducts(extractedItem.ProductName, activeProducts);

            if (candidateProducts.Count == 0)
            {
                itemDto.ResolutionStatus = "NotFound";
                itemDto.ProductID = null;
                itemDto.ProductName = null;
                itemDto.Message = $"Product '{extractedItem.ProductName}' not found in active catalog.";
            }
            else if (candidateProducts.Count > 1)
            {
                itemDto.ResolutionStatus = "Ambiguous";
                itemDto.ProductID = null;
                itemDto.ProductName = null;
                itemDto.AmbiguousMatches = candidateProducts.Select(p => p.ProductName).Distinct().ToList();
                itemDto.Message = $"Multiple active products matched '{extractedItem.ProductName}'.";
            }
            else
            {
                var resolvedProduct = candidateProducts[0];
                itemDto.ResolutionStatus = "Resolved";
                itemDto.ProductID = resolvedProduct.ProductID; // Authoritative DB ProductID
                itemDto.ProductName = resolvedProduct.ProductName; // Authoritative DB ProductName
                itemDto.Unit = resolvedProduct.Unit; // Authoritative DB Unit
                itemDto.Message = "Product resolved successfully.";
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

        // Response Construction
        return new ParseVoiceProcurementOrderResponse
        {
            Success = true,
            Message = "Procurement order parsed and resolved successfully.",
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
        string normalized = Regex.Replace(raw, @"\b(?:outlet|branch|store|location)\b", "").Trim();

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

    private static List<Product> FindCandidateProducts(string spokenName, List<Product> activeProducts)
    {
        string raw = spokenName.Trim().ToLowerInvariant();
        string root = GetWordRoot(raw);

        // 1. Exact full-name match check
        var exactFull = activeProducts
            .Where(p => string.Equals(p.ProductName.Trim(), raw, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (exactFull.Count == 1)
        {
            return exactFull;
        }

        // 2. Gather candidates where spoken name or root matches full name or token
        var candidates = new List<Product>();

        foreach (var p in activeProducts)
        {
            string pName = p.ProductName.Trim().ToLowerInvariant();
            string pRoot = GetWordRoot(pName);

            if (string.Equals(pName, raw, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(pRoot, root, StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add(p);
                continue;
            }

            var pWords = pName.Split(new[] { ' ', '-', '/', ',' }, StringSplitOptions.RemoveEmptyEntries);
            bool matched = pWords.Any(w =>
            {
                string wLower = w.ToLowerInvariant();
                string wRoot = GetWordRoot(wLower);
                return string.Equals(wLower, raw, StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(wRoot, root, StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(wLower, root, StringComparison.OrdinalIgnoreCase) ||
                       (!string.IsNullOrWhiteSpace(root) && root.Length > 2 && wLower.StartsWith(root, StringComparison.OrdinalIgnoreCase));
            });

            if (matched)
            {
                candidates.Add(p);
            }
        }

        return candidates.GroupBy(p => p.ProductID).Select(g => g.First()).ToList();
    }

    private static string GetWordRoot(string word)
    {
        string w = word.Trim().ToLowerInvariant();
        if (w.EndsWith("oes") && w.Length > 4)
            return w.Substring(0, w.Length - 2); // potatoes -> potato, tomatoes -> tomato
        if (w.EndsWith("ies") && w.Length > 4)
            return w.Substring(0, w.Length - 3) + "y"; // berries -> berry
        if (w.EndsWith("es") && w.Length > 3)
            return w.Substring(0, w.Length - 2); // mangoes -> mango, oranges -> orange (oranges - s)
        if (w.EndsWith("s") && !w.EndsWith("ss") && w.Length > 3)
            return w.Substring(0, w.Length - 1); // carrots -> carrot, onions -> onion
        return w;
    }
}
