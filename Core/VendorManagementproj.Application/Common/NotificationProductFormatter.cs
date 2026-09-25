using System;
using System.Collections.Generic;
using System.Linq;
using VendorManagementproj.Domain.Entities;

namespace VendorManagementproj.Application.Common;

public static class NotificationProductFormatter
{
    public static (string TitleSummary, string MessageSummary) FormatProductSummaries(
        PurchaseRequest? purchaseRequest,
        IDictionary<int, string>? fallbackProductNames = null)
    {
        return FormatProductSummaries(purchaseRequest?.Items, fallbackProductNames);
    }

    public static (string TitleSummary, string MessageSummary) FormatProductSummaries(
        IEnumerable<PurchaseRequestItem>? items,
        IDictionary<int, string>? fallbackProductNames = null)
    {
        if (items == null)
            return (string.Empty, string.Empty);

        var tuples = items.Select(item =>
        {
            string? prodName = item.Product?.ProductName;
            if (string.IsNullOrWhiteSpace(prodName) && fallbackProductNames != null && fallbackProductNames.TryGetValue(item.ProductID, out var fbName))
            {
                prodName = fbName;
            }
            if (string.IsNullOrWhiteSpace(prodName))
            {
                prodName = $"Product #{item.ProductID}";
            }
            string? unit = item.Unit ?? item.Product?.Unit;
            return ((string?)prodName, item.Quantity, (string?)unit);
        });

        return FormatInternal(tuples);
    }

    public static (string TitleSummary, string MessageSummary) FormatProductSummaries(
        IEnumerable<(string? ProductName, decimal Quantity, string? Unit)>? items)
    {
        if (items == null)
            return (string.Empty, string.Empty);

        return FormatInternal(items);
    }

    private static (string TitleSummary, string MessageSummary) FormatInternal(
        IEnumerable<(string? ProductName, decimal Quantity, string? Unit)> itemList)
    {
        var titleParts = new List<string>();
        var msgParts = new List<string>();

        foreach (var (prodName, qty, unit) in itemList)
        {
            string cleanProdName = string.IsNullOrWhiteSpace(prodName) ? "Product" : prodName.Trim();
            string qtyStr = qty % 1 == 0
                ? qty.ToString("0")
                : qty.ToString("0.##");

            string unitStr = (unit ?? string.Empty).Trim();
            string qtyWithUnit = string.IsNullOrEmpty(unitStr) ? qtyStr : $"{qtyStr} {unitStr}";

            titleParts.Add($"{cleanProdName} ({qtyWithUnit})");
            msgParts.Add($"{cleanProdName}, {qtyWithUnit}");
        }

        if (titleParts.Count == 0)
            return (string.Empty, string.Empty);

        string titleSummary = string.Join(", ", titleParts);

        string msgSummary;
        if (msgParts.Count == 0)
        {
            msgSummary = string.Empty;
        }
        else if (msgParts.Count == 1)
        {
            msgSummary = msgParts[0];
        }
        else if (msgParts.Count == 2)
        {
            msgSummary = $"{msgParts[0]} and {msgParts[1]}";
        }
        else
        {
            msgSummary = $"{string.Join(", ", msgParts.Take(msgParts.Count - 1))} and {msgParts.Last()}";
        }

        return (titleSummary, msgSummary);
    }

    public static string FormatProductTitle(IEnumerable<(string? ProductName, decimal Quantity, string? Unit)>? items)
    {
        return FormatProductSummaries(items).TitleSummary;
    }

    public static string FormatProductMessage(IEnumerable<(string? ProductName, decimal Quantity, string? Unit)>? items)
    {
        return FormatProductSummaries(items).MessageSummary;
    }

    public static string FormatProductTitle(IEnumerable<PurchaseRequestItem>? items, IDictionary<int, string>? fallbackProductNames = null)
    {
        return FormatProductSummaries(items, fallbackProductNames).TitleSummary;
    }

    public static string FormatProductMessage(IEnumerable<PurchaseRequestItem>? items, IDictionary<int, string>? fallbackProductNames = null)
    {
        return FormatProductSummaries(items, fallbackProductNames).MessageSummary;
    }
}


