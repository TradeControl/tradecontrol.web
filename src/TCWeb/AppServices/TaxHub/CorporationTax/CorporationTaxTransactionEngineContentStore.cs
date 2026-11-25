using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using TradeControl.Tax.UK.Adapters.Submission.Audit;
using TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

namespace TradeControl.Web.AppServices.TaxHub.CorporationTax;

/// <summary>
/// Adapts the shared protected-content boundary to the Transaction Engine. The
/// returned handle carries the immutable digest needed to perform a verified read;
/// XML and authority responses never enter workflow metadata.
/// </summary>
internal sealed class CorporationTaxTransactionEngineContentStore(
    ICompaniesHouseProtectedContentStore protectedContent,
    IDataProtectionProvider dataProtection) : ISubmissionContentStore
{
    private const string ProtectionPurpose =
        "TradeControl.Web.TaxHub.CorporationTax.TransactionEngine.v1";

    public async Task<string> StoreAsync(string tenantReference, string principalReference,
        SubmissionContentKind kind, ReadOnlyMemory<byte> content,
        CancellationToken cancellationToken = default)
    {
        ValidateReference(principalReference, nameof(principalReference));
        var protectedBytes = Protector(tenantReference, principalReference).Protect(content.ToArray());
        var retained = await protectedContent.WriteAsync(tenantReference,
            kind == SubmissionContentKind.Payload
                ? "ct-transaction-payload"
                : "ct-transaction-response",
            protectedBytes, cancellationToken);
        return $"{retained.Reference}:{retained.Sha256}";
    }

    public async Task<byte[]?> ReadAsync(string tenantReference, string principalReference,
        string contentReference, CancellationToken cancellationToken = default)
    {
        ValidateReference(principalReference, nameof(principalReference));
        var separator = contentReference.LastIndexOf(':');
        if (separator < 1 || separator == contentReference.Length - 1)
            return null;
        var reference = contentReference[..separator];
        var sha256 = contentReference[(separator + 1)..];
        if (sha256.Length != 64 || sha256.Any(character => !Uri.IsHexDigit(character)))
            return null;
        var protectedBytes = (await protectedContent.ReadAsync(tenantReference, reference, sha256,
            cancellationToken)).Bytes;
        try
        {
            return Protector(tenantReference, principalReference).Unprotect(protectedBytes);
        }
        catch (CryptographicException exception)
        {
            throw new InvalidDataException(
                "Corporation Tax transaction evidence could not be decrypted for its owner.",
                exception);
        }
    }

    private IDataProtector Protector(string tenantReference, string principalReference) =>
        dataProtection.CreateProtector(ProtectionPurpose, tenantReference, principalReference);

    private static void ValidateReference(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 256)
            throw new ArgumentException("A bounded owner reference is required.", parameterName);
    }
}
