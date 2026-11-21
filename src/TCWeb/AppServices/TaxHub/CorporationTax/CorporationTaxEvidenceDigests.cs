using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TradeControl.Tax.UK.Application.DataProvision;

namespace TradeControl.Web.AppServices.TaxHub.CorporationTax;

internal static class CorporationTaxEvidenceDigests
{
    internal static string SourceSha256(IEnumerable<SourceVersion> versions)
    {
        var canonical = string.Join("\n", versions
            .OrderBy(value => value.SourceCode, StringComparer.Ordinal)
            .ThenBy(value => value.RowVersion, StringComparer.Ordinal)
            .Select(value => $"{value.SourceCode}|{value.RowVersion}|{value.UpdatedOn:O}"));
        return Hash(canonical);
    }

    internal static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
