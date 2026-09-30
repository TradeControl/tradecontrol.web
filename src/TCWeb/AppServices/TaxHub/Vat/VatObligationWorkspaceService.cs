using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradeControl.Web.Data;

namespace TradeControl.Web.AppServices.TaxHub.Vat;

public enum VatObligationWorkspaceState
{
    Ready,
    ConfigurationRequired,
    ReauthorisationRequired,
    Unavailable,
    AuthorityError
}

public enum VatObligationMatchState
{
    OpenMatched,
    OpenLocalPeriodMissing,
    AuthorityReturnAlreadyFiled,
    Fulfilled,
    LocalWithoutAuthorityObligation
}

public sealed record VatLocalSubmissionPeriod(DateOnly Start, DateOnly End);

public sealed record VatObligationWorkspaceRow(
    string? PeriodKey,
    DateOnly Start,
    DateOnly End,
    DateOnly? Due,
    DateOnly? Received,
    VatObligationMatchState MatchState)
{
    public bool CanReview => MatchState == VatObligationMatchState.OpenMatched;
}

public sealed record VatObligationWorkspaceResult(
    VatObligationWorkspaceState State,
    IReadOnlyList<VatObligationWorkspaceRow> Rows,
    string? SafeMessage = null,
    string? SupportReference = null);

public interface IVatAuthorityObligationSource
{
    Task<IReadOnlyList<VatAuthorityObligation>> RetrieveAsync(VatWorkflowIdentity identity,
        string vrn, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}

public interface IVatAuthorityReturnReadbackSource
{
    Task<bool> ExistsAsync(VatWorkflowIdentity identity, string vrn, string periodKey,
        CancellationToken cancellationToken = default);
}

public interface IVatObligationWorkspaceService
{
    Task<VatObligationWorkspaceResult> GetAsync(CancellationToken cancellationToken = default);
}

public sealed class VatAuthorityRequestException(string safeCode, string? supportReference = null)
    : Exception("The HMRC VAT obligation request did not complete successfully.")
{
    public string SafeCode { get; } = safeCode;
    public string? SupportReference { get; } = supportReference;
}

public sealed class VatObligationReconciler
{
    public IReadOnlyList<VatObligationWorkspaceRow> Reconcile(
        IReadOnlyCollection<VatAuthorityObligation> authority,
        IReadOnlyCollection<VatLocalSubmissionPeriod> local)
    {
        ArgumentNullException.ThrowIfNull(authority);
        ArgumentNullException.ThrowIfNull(local);
        var localKeys = local.Select(item => (item.Start, item.End)).ToHashSet();
        var authorityKeys = authority.Select(item => (item.Start, item.End)).ToHashSet();
        var rows = authority.Select(item => new VatObligationWorkspaceRow(
                item.PeriodKey, item.Start, item.End, item.Due, item.Received,
                item.Status.Equals("F", StringComparison.OrdinalIgnoreCase)
                    ? VatObligationMatchState.Fulfilled
                    : localKeys.Contains((item.Start, item.End))
                        ? VatObligationMatchState.OpenMatched
                        : VatObligationMatchState.OpenLocalPeriodMissing))
            .Concat(local.Where(item => !authorityKeys.Contains((item.Start, item.End)))
                .Select(item => new VatObligationWorkspaceRow(null, item.Start, item.End, null, null,
                    VatObligationMatchState.LocalWithoutAuthorityObligation)))
            .OrderBy(item => item.MatchState == VatObligationMatchState.OpenMatched ? 0
                : item.MatchState == VatObligationMatchState.OpenLocalPeriodMissing ? 1
                : item.MatchState == VatObligationMatchState.Fulfilled ? 2 : 3)
            .ThenByDescending(item => item.End)
            .ToArray();
        return rows;
    }
}

public sealed class VatObligationWorkspaceService(
    NodeContext nodeContext,
    IVatWorkflowIdentityAccessor identities,
    IVatAuthorityObligationSource authority,
    IVatAuthorityReturnReadbackSource readback,
    VatObligationReconciler reconciler,
    TimeProvider timeProvider,
    IOptions<VatProductHostOptions> hostOptions,
    ILogger<VatObligationWorkspaceService> logger) : IVatObligationWorkspaceService
{
    internal const string SyntheticPlaceholderVrn = "999000001";

    public static (DateOnly From, DateOnly To) SearchWindow(DateOnly today) =>
        (today.AddDays(-365), today);

    public async Task<VatObligationWorkspaceResult> GetAsync(CancellationToken cancellationToken = default)
    {
        var identity = await identities.GetRequiredAsync(cancellationToken);
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var activeStatusCodes = await nodeContext.App_tbStatutoryStatuses.AsNoTracking()
            .Where(item => item.IsActive).Select(item => item.StatusCode).ToArrayAsync(cancellationToken);
        var profile = await ReadVatIdentityAsync(identity.ReportingSubjectReference, today, cancellationToken);

        if (profile is null || !profile.IsReviewed || !activeStatusCodes.Contains(profile.StatusCode)
            || !profile.IsConsistent
            || profile.AuthorityReference is null || profile.AuthorityReference.Length != 9
            || profile.AuthorityReference.Any(character => !char.IsDigit(character))
            || profile.AuthorityReference == SyntheticPlaceholderVrn)
            return new(VatObligationWorkspaceState.ConfigurationRequired, [],
                "Configure and activate a reviewed indirect-tax reporting profile that matches the reporting subject VAT registration number.");

        try
        {
            // HMRC's sandbox exposes deterministic historical obligations. The optional anchor is
            // accepted only by the DevelopmentFiles sandbox composition; all operational clocks
            // (OAuth, fraud evidence and authority communication) remain on real current time.
            var window = SearchWindow(hostOptions.Value.SandboxObligationAsOfDate ?? today);
            var obligations = await authority.RetrieveAsync(identity, profile.AuthorityReference,
                window.From, window.To, cancellationToken);
            var local = await ReadLocalPeriodsAsync(cancellationToken);
            var rows = reconciler.Reconcile(obligations, local).ToArray();
            foreach (var row in rows.Where(item => item.CanReview && item.PeriodKey is not null))
                if (await readback.ExistsAsync(identity, profile.AuthorityReference, row.PeriodKey!, cancellationToken))
                    rows[Array.IndexOf(rows, row)] = row with
                    {
                        MatchState = VatObligationMatchState.AuthorityReturnAlreadyFiled
                    };
            return new(VatObligationWorkspaceState.Ready, rows);
        }
        catch (VatAuthorityRequestException exception)
        {
            logger.LogWarning(exception,
                "HMRC VAT obligations failed with code {SafeCode} and support reference {SupportReference}.",
                exception.SafeCode, exception.SupportReference);
            await RecordAuthorityFailureAsync(exception, cancellationToken);
            var reauthorisation = exception.SafeCode.StartsWith("OAUTH-REAUTHORISATION-", StringComparison.Ordinal);
            var unavailable = exception.SafeCode is "VAT-HOST-DISABLED" or "VAT-AUTHORITY-TOPOLOGY-UNAVAILABLE";
            return new(reauthorisation ? VatObligationWorkspaceState.ReauthorisationRequired
                    : unavailable ? VatObligationWorkspaceState.Unavailable
                    : VatObligationWorkspaceState.AuthorityError, [],
                reauthorisation
                    ? "HMRC requires the business to reconnect before obligations can be retrieved."
                    : unavailable
                        ? "HMRC VAT obligations are not configured for this deployment."
                    : "HMRC VAT obligations could not be refreshed. Try again later.",
                exception.SupportReference);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected failure while loading the HMRC VAT obligations workspace.");
            var supportReference = await RecordUnexpectedFailureAsync(exception, cancellationToken);
            return new(VatObligationWorkspaceState.AuthorityError, [],
                "HMRC VAT obligations could not be refreshed. Try again later.", supportReference);
        }
    }

    private sealed record VatIdentity(string? AuthorityReference, short StatusCode,
        bool IsReviewed, bool IsConsistent);

    private async Task<VatIdentity?> ReadVatIdentityAsync(string subjectCode, DateOnly asOfDate,
        CancellationToken cancellationToken)
    {
        const string sql = """
SELECT TOP (1) AuthorityReference, StatusCode, IsReviewed, IsConsistent
FROM Cash.vwTaxVatIdentity
WHERE SubjectCode = @SubjectCode
  AND ValidFrom <= @AsOfDate
  AND (ValidTo IS NULL OR ValidTo >= @AsOfDate)
ORDER BY ValidFrom DESC;
""";
        var connection = nodeContext.Database.GetDbConnection();
        var close = connection.State != ConnectionState.Open;
        if (close) await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.Add(new SqlParameter("@SubjectCode", SqlDbType.NVarChar, 50) { Value = subjectCode });
            command.Parameters.Add(new SqlParameter("@AsOfDate", SqlDbType.Date)
                { Value = asOfDate.ToDateTime(TimeOnly.MinValue) });
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken)
                ? new(reader.IsDBNull(0) ? null : reader.GetString(0), reader.GetInt16(1),
                    reader.GetBoolean(2), reader.GetBoolean(3))
                : null;
        }
        finally
        {
            if (close) await connection.CloseAsync();
        }
    }

    private async Task RecordAuthorityFailureAsync(VatAuthorityRequestException exception,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await nodeContext.EventLog(NodeEnum.EventType.IsError,
                $"Tax Hub HMRC VAT obligations failed. Code: {exception.SafeCode}. "
                + $"Support reference: {exception.SupportReference ?? "not supplied"}.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception logException)
        {
            logger.LogError(logException, "Failed to write an HMRC VAT obligations failure to the node Event Log.");
        }
    }

    private async Task<string?> RecordUnexpectedFailureAsync(Exception exception,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await nodeContext.ErrorLog(exception);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception logException)
        {
            logger.LogError(logException, "Failed to write an unexpected VAT workspace failure to the node Event Log.");
            return null;
        }
    }

    private async Task<IReadOnlyList<VatLocalSubmissionPeriod>> ReadLocalPeriodsAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
SELECT DISTINCT CONVERT(date, due.PayFrom) AS PeriodStart,
       CONVERT(date, DATEADD(day, -1, due.PayTo)) AS PeriodEnd
FROM Cash.vwTaxVatSubmission submission
JOIN Cash.fnTaxTypeDueDates(1, 0) due ON submission.StartOn = due.PayTo
ORDER BY PeriodEnd DESC;
""";
        var result = new List<VatLocalSubmissionPeriod>();
        var connection = nodeContext.Database.GetDbConnection();
        var close = connection.State != ConnectionState.Open;
        if (close) await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                result.Add(new(DateOnly.FromDateTime(reader.GetDateTime(0)),
                    DateOnly.FromDateTime(reader.GetDateTime(1))));
        }
        finally
        {
            if (close) await connection.CloseAsync();
        }
        return result;
    }
}
