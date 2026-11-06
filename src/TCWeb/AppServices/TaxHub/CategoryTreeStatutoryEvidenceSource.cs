using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeControl.Tax.UK.Application.StatutoryEvidence;
using TradeControl.Web.Data;

namespace TradeControl.Web.AppServices.TaxHub;

public interface ICategoryTreeStatutoryEvidenceSource
{
    Task<CategoryTreeSealEvidence> ReadActiveAsync(CancellationToken cancellationToken = default);
}

internal sealed class CategoryTreeStatutoryEvidenceSource(NodeContext nodeContext)
    : ICategoryTreeStatutoryEvidenceSource
{
    public async Task<CategoryTreeSealEvidence> ReadActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await (
            from options in nodeContext.App_tbOptions.AsNoTracking()
            join version in nodeContext.Cash_tbCategoryVersions.AsNoTracking()
                on options.CategoryVersionId equals version.VersionId
            select new
            {
                version.VersionId,
                version.VersionStatusCode,
                version.CanonicalProfile,
                version.CanonicalSha256,
                version.CanonicalByteLength
            }).ToArrayAsync(cancellationToken);

        if (rows.Length != 1)
            throw new InvalidOperationException(
                "The active Category Tree could not be resolved from the singleton application options row.");

        var row = rows[0];
        if (row.VersionStatusCode is not ((short)NodeEnum.CategoryVersionStatus.Published)
            and not ((short)NodeEnum.CategoryVersionStatus.Submitted))
            throw new InvalidOperationException(
                "The active Category Tree is not eligible for statutory preparation.");
        if (row.CanonicalSha256 is not { Length: 32 }
            || !row.CanonicalByteLength.HasValue)
            throw new InvalidOperationException(
                "The active Category Tree does not have complete canonical evidence.");

        var evidence = new CategoryTreeSealEvidence(
            row.VersionId.Trim(),
            row.CanonicalProfile.Trim(),
            Convert.ToHexString(row.CanonicalSha256),
            row.CanonicalByteLength.Value);
        CategoryTreeStatutoryEvidence.Validate(evidence);
        return evidence;
    }
}
