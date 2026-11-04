using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using CategoryTreeStatus = TradeControl.Web.Data.NodeEnum.CategoryVersionStatus;

namespace TradeControl.Web.AppServices.CategoryTree;

public static class CategoryTreeLifecycleContract
{
    public static bool CanBeSelectedAsActive(CategoryTreeStatus status) =>
        status is CategoryTreeStatus.Published or CategoryTreeStatus.Submitted;

    public static bool IsActive(CategoryTreeStatus status, bool isSelectedByOptions) =>
        isSelectedByOptions && CanBeSelectedAsActive(status);

    public static bool CanTransition(CategoryTreeStatus from, CategoryTreeStatus to) => (from, to) switch
    {
        (CategoryTreeStatus.Draft, CategoryTreeStatus.Published) => true,
        (CategoryTreeStatus.Published, CategoryTreeStatus.Submitted) => true,
        (CategoryTreeStatus.Published, CategoryTreeStatus.Archived) => true,
        (CategoryTreeStatus.Submitted, CategoryTreeStatus.Archived) => true,
        _ => false
    };

    public static bool PredecessorStillMatches(string baseSha256, string activeSha256) =>
        IsSha256(baseSha256) && IsSha256(activeSha256)
        && CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(baseSha256), Convert.FromHexString(activeSha256));

    private static bool IsSha256(string? value) => value is { Length: 64 } && value.All(Uri.IsHexDigit);
}

public sealed record CategoryTreeCategoryContract(string CategoryCode, string Category,
    short CategoryTypeCode, short CashPolarityCode, short CashTypeCode, short DisplayOrder, bool IsEnabled);
public sealed record CategoryTreeTotalContract(string ParentCode, string ChildCode, short DisplayOrder);
public sealed record CategoryTreeCodeMapContract(string CashCode, string CategoryCode);
public sealed record CategoryTreeRootContract(string RootType, string CategoryCode);
public sealed record CategoryTreeContentContract(
    IReadOnlyCollection<CategoryTreeCategoryContract> Categories,
    IReadOnlyCollection<CategoryTreeTotalContract> Totals,
    IReadOnlyCollection<CategoryTreeCodeMapContract> CodeMap,
    IReadOnlyCollection<CategoryTreeRootContract> Roots);
public sealed record CategoryTreeCanonicalContent(byte[] Bytes, string Sha256)
{
    public long ByteLength => Bytes.LongLength;
}

public static class CategoryTreeCanonicalizer
{
    public const string Profile = "trade-control-category-tree-v1";
    public const string NetProfitRoot = "NET_PROFIT";
    public const string VatRoot = "VAT";

    public static CategoryTreeCanonicalContent Create(CategoryTreeContentContract content)
    {
        ArgumentNullException.ThrowIfNull(content);
        Validate(content);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }))
        {
            writer.WriteStartObject();
            writer.WriteString("profile", Profile);
            writer.WritePropertyName("categories");
            writer.WriteStartArray();
            foreach (var x in content.Categories.OrderBy(x => x.CategoryCode, StringComparer.Ordinal))
            {
                writer.WriteStartObject();
                writer.WriteString("categoryCode", Nfc(x.CategoryCode));
                writer.WriteString("category", Nfc(x.Category));
                writer.WriteNumber("categoryTypeCode", x.CategoryTypeCode);
                writer.WriteNumber("cashPolarityCode", x.CashPolarityCode);
                writer.WriteNumber("cashTypeCode", x.CashTypeCode);
                writer.WriteNumber("displayOrder", x.DisplayOrder);
                writer.WriteBoolean("isEnabled", x.IsEnabled);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WritePropertyName("totals");
            writer.WriteStartArray();
            foreach (var x in content.Totals.OrderBy(x => x.ParentCode, StringComparer.Ordinal)
                         .ThenBy(x => x.ChildCode, StringComparer.Ordinal))
            {
                writer.WriteStartObject();
                writer.WriteString("parentCode", Nfc(x.ParentCode));
                writer.WriteString("childCode", Nfc(x.ChildCode));
                writer.WriteNumber("displayOrder", x.DisplayOrder);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WritePropertyName("codeMap");
            writer.WriteStartArray();
            foreach (var x in content.CodeMap.OrderBy(x => x.CashCode, StringComparer.Ordinal))
            {
                writer.WriteStartObject();
                writer.WriteString("cashCode", Nfc(x.CashCode));
                writer.WriteString("categoryCode", Nfc(x.CategoryCode));
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WritePropertyName("roots");
            writer.WriteStartArray();
            foreach (var x in content.Roots.OrderBy(x => RootOrder(x.RootType))
                         .ThenBy(x => x.RootType, StringComparer.Ordinal))
            {
                writer.WriteStartObject();
                writer.WriteString("rootType", Nfc(x.RootType));
                writer.WriteString("categoryCode", Nfc(x.CategoryCode));
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        var bytes = stream.ToArray();
        return new(bytes, Convert.ToHexString(SHA256.HashData(bytes)));
    }

    private static void Validate(CategoryTreeContentContract content)
    {
        NonEmpty(content.Categories, nameof(content.Categories));
        NonEmpty(content.Roots, nameof(content.Roots));
        ArgumentNullException.ThrowIfNull(content.Totals);
        ArgumentNullException.ThrowIfNull(content.CodeMap);
        Unique(content.Categories, x => Required(x.CategoryCode), "category codes");
        Unique(content.Totals, x => $"{Required(x.ParentCode)}\0{Required(x.ChildCode)}", "total edges");
        Unique(content.CodeMap, x => Required(x.CashCode), "Cash Code mappings");
        Unique(content.Roots, x => Required(x.RootType), "root types");
        foreach (var x in content.Categories) _ = Required(x.Category);
        foreach (var x in content.CodeMap) _ = Required(x.CategoryCode);
        foreach (var x in content.Roots) _ = Required(x.CategoryCode);
        if (!content.Roots.Select(x => x.RootType).ToHashSet(StringComparer.Ordinal)
                .SetEquals([NetProfitRoot, VatRoot]))
            throw new ArgumentException("The v1 profile requires exactly NET_PROFIT and VAT roots.");
    }

    private static void NonEmpty<T>(IReadOnlyCollection<T>? values, string name)
    {
        if (values is null || values.Count == 0) throw new ArgumentException("A non-empty collection is required.", name);
    }
    private static void Unique<T>(IEnumerable<T> values, Func<T, string> key, string description)
    {
        var keys = values.Select(key).ToArray();
        if (keys.Distinct(StringComparer.Ordinal).Count() != keys.Length)
            throw new ArgumentException($"Category Tree {description} must be unique.");
    }
    private static string Required(string? value) => string.IsNullOrWhiteSpace(value)
        ? throw new ArgumentException("A Category Tree value is required.") : Nfc(value);
    private static string Nfc(string value) => value.Normalize(NormalizationForm.FormC);
    private static int RootOrder(string value) => value switch { NetProfitRoot => 0, VatRoot => 1, _ => 2 };
}
