using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TradeControl.Tax.UK.Adapters.TradeControl.Data;
using TradeControl.Tax.UK.Adapters.TradeControl.Readers;
using TradeControl.Tax.UK.Application.DataProvision;
using TradeControl.Web.AppServices.TaxHub.CompaniesHouse;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Pages.Tax.Hub.Models;

namespace TradeControl.Web.AppServices.TaxHub.CorporationTax;

internal sealed record CorporationTaxAccountsReconciliation(
    decimal AcceptedAccountingProfit,
    decimal CurrentAccountingProfit,
    decimal AccountingProfitDifference,
    decimal AcceptedTaxCharge,
    decimal CurrentTaxCharge,
    decimal TaxChargeDifference,
    decimal Tolerance,
    bool IsReconciled);

public sealed record CorporationTaxPeriodCalculationReview(
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal AccountingProfit,
    decimal DepreciationAddBack,
    decimal OtherAddBacks,
    decimal Deductions,
    decimal CapitalAllowances,
    decimal ChargeableGains,
    decimal LossesUsed,
    decimal TaxableTotalProfits,
    decimal RelevantDistributions,
    decimal AugmentedProfits,
    int AssociatedCompanyCount,
    string CalculationCode,
    decimal TaxBeforeMarginalRelief,
    decimal MarginalRelief,
    decimal CorporationTaxChargeable,
    decimal OtherReliefs,
    decimal TaxPayable,
    bool IsFilingReady,
    CorporationTaxPeriodReviewState Review);

public sealed record CorporationTaxPeriodReviewState(
    CorporationTaxPeriodReviewedInput Input,
    int Revision,
    string InputSha256,
    string? ReviewedBy,
    DateTimeOffset? ReviewedAtUtc,
    bool IsReviewed,
    bool IsStale);

public sealed record CorporationTaxPeriodReviewSaveRequest(
    string LogicalFilingIdentity,
    CorporationTaxPeriodReviewedInput Input,
    int ExpectedRevision);

internal sealed record CorporationTaxCalculationReview(
    string AccountsSnapshotSha256,
    string SourceSha256,
    CorporationTaxAccountsReconciliation Reconciliation,
    IReadOnlyList<CorporationTaxPeriodCalculationReview> Periods,
    bool AcceptanceSimulated,
    bool IsReady);

internal interface ICorporationTaxCalculationReviewService
{
    Task<CorporationTaxCalculationReview> BuildAsync(
        TaxHubCorporationTaxContinuation continuation,
        CancellationToken cancellationToken = default);

    Task<CorporationTaxCalculationReview> SavePeriodAsync(
        ClaimsPrincipal principal,
        TaxHubCorporationTaxContinuation continuation,
        string logicalFilingIdentity,
        CorporationTaxPeriodReviewedInput input,
        int expectedRevision,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Recalculates a non-filing CT comparison from current Trade Control transactions and compares it
/// with the exact retained full-accounts artifact. It does not prepare or mutate a filing artifact.
/// </summary>
internal sealed class CorporationTaxCalculationReviewService(
    NodeContext nodeContext,
    ICompaniesHouseAccountsSnapshotService accountsSnapshots,
    ICorporationTaxReviewStore reviewStore,
    ICompaniesHouseWorkflowIdentityAccessor identityAccessor,
    IOptions<CompaniesHouseProductHostOptions> hostOptions,
    TimeProvider timeProvider) : ICorporationTaxCalculationReviewService
{
    private const decimal ReconciliationTolerance = 0.10m;

    public async Task<CorporationTaxCalculationReview> BuildAsync(
        TaxHubCorporationTaxContinuation continuation,
        CancellationToken cancellationToken = default) =>
        await BuildAsync(continuation, null, cancellationToken);

    private async Task<CorporationTaxCalculationReview> BuildAsync(
        TaxHubCorporationTaxContinuation continuation,
        CorporationTaxPeriodReviewSaveRequest? proposedReview,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(continuation);
        var allowSimulation = continuation.AcceptanceSimulated
            && hostOptions.Value.CanSimulateOfficialTestAcceptance;
        var accepted = await accountsSnapshots.ReadAcceptedForCorporationTaxAsync(
            continuation.AccountsApprovalReference, allowSimulation, cancellationToken);
        var accounts = accepted.Accounts;
        if (accepted.AcceptanceSimulated != continuation.AcceptanceSimulated
            || !string.Equals(accepted.ConversationReference,
                continuation.AccountsConversationReference, StringComparison.Ordinal)
            || accounts.PeriodStart != continuation.AccountsPeriodStart
            || accounts.PeriodEnd != continuation.AccountsPeriodEnd
            || !string.Equals(accounts.AccountsSnapshotSha256,
                continuation.AccountsSnapshotSha256, StringComparison.Ordinal)
            || !string.Equals(accounts.FullAccountsSha256,
                continuation.FullAccountsSha256, StringComparison.Ordinal))
            throw new InvalidOperationException(
                "The Corporation Tax calculation scope no longer matches its accepted accounts evidence.");

        var connectionString = nodeContext.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("The Corporation Tax accounting source is unavailable.");

        var filingPeriods = continuation.Periods?.ToArray() ?? [];
        if (filingPeriods.Length is < 1 or > 2)
            throw new InvalidOperationException("The Corporation Tax filing scope has no valid accounting periods.");

        var factory = new ConnectionFactory();
        var taxReader = new TcBusinessTaxReader(factory);
        var reviewedPeriods = new List<CorporationTaxPeriodReviewedInput>(filingPeriods.Length);
        var storedReviews = new Dictionary<ReportingWindow, CorporationTaxReviewedPeriodRecord?>();
        var proposedWindows = new HashSet<ReportingWindow>();
        var identity = await identityAccessor.GetRequiredAsync(cancellationToken);
        foreach (var period in filingPeriods)
        {
            var liability = await taxReader.ReadCorporationTaxLiabilityAsync(connectionString,
                period.PeriodStart.ToDateTime(TimeOnly.MinValue),
                period.PeriodEnd.ToDateTime(TimeOnly.MinValue), cancellationToken);
            var window = new ReportingWindow(period.PeriodStart, period.PeriodEnd);
            var stored = await reviewStore.GetAsync(identity.TenantReference,
                period.LogicalIdentity, cancellationToken);
            if (stored is not null
                && (!string.Equals(stored.TenantReference,
                        identity.TenantReference, StringComparison.Ordinal)
                    || !string.Equals(stored.AccountsSnapshotSha256,
                        continuation.AccountsSnapshotSha256, StringComparison.Ordinal)
                    || stored.PeriodStart != period.PeriodStart
                    || stored.PeriodEnd != period.PeriodEnd
                    || !string.Equals(stored.LogicalFilingIdentity,
                        period.LogicalIdentity, StringComparison.Ordinal)))
                throw new InvalidOperationException(
                    "The saved Corporation Tax review does not match its filing scope.");
            var isProposed = proposedReview is not null
                && string.Equals(proposedReview.LogicalFilingIdentity,
                    period.LogicalIdentity, StringComparison.Ordinal);
            var reviewed = isProposed
                ? proposedReview!.Input
                : stored?.Input ?? new CorporationTaxPeriodReviewedInput(
                    window, [], [], new(0m, 0m, 0m), liability.LossUsed, 0m, 0m, null);
            Validate(reviewed);
            reviewedPeriods.Add(reviewed);
            storedReviews.Add(window, stored);
            if (isProposed) proposedWindows.Add(window);
        }

        var source = await new TcCorporationTaxSourceReader(factory, connectionString).ReadAsync(new(
            continuation.AccountsPeriodEnd,
            continuation.AccountsPeriodEnd,
            new(continuation.AccountsPeriodStart, continuation.AccountsPeriodEnd),
            new(null, string.Empty, reviewedPeriods, string.Empty,
                continuation.AccountsPeriodEnd)), cancellationToken);

        var sourceSha256 = CorporationTaxEvidenceDigests.SourceSha256(source.Versions);
        var periods = source.CorporationTaxPeriods.Select(period =>
        {
            var liability = period.Liability.Value;
            var stored = storedReviews[period.Period];
            var isProposed = proposedWindows.Contains(period.Period);
            var input = isProposed
                ? reviewedPeriods.Single(value => value.Period == period.Period)
                : stored?.Input ?? reviewedPeriods.Single(value => value.Period == period.Period);
            var inputSha256 = InputSha256(input);
            if (!isProposed && stored is not null
                && !string.Equals(stored.InputSha256, inputSha256, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "The saved Corporation Tax review failed its input integrity check.");
            var review = new CorporationTaxPeriodReviewState(
                input,
                isProposed ? proposedReview!.ExpectedRevision + 1 : stored?.Revision ?? 0,
                inputSha256,
                isProposed ? null : stored?.ReviewedBy,
                isProposed ? null : stored?.ReviewedAtUtc,
                isProposed || stored is not null,
                !isProposed && stored is not null
                    && !string.Equals(stored.SourceSha256, sourceSha256, StringComparison.Ordinal));
            return new CorporationTaxPeriodCalculationReview(
                period.Period.Start,
                period.Period.End,
                period.AccountsProfitLossBeforeTax.Value,
                period.AddBacks.Where(value => value.Value.Description == "Accounting depreciation")
                    .Sum(value => value.Value.Amount),
                period.AddBacks.Where(value => value.Value.Description != "Accounting depreciation")
                    .Sum(value => value.Value.Amount),
                period.Deductions.Sum(value => value.Value.Amount),
                period.CapitalAllowances.Value.WritingDownAllowance
                    + period.CapitalAllowances.Value.AnnualInvestmentAllowance
                    + period.CapitalAllowances.Value.OtherAllowances,
                period.ChargeableGains.Value,
                period.LossRelief.Value.Used,
                period.TaxableTotalProfits.Value,
                liability.RelevantDistributions,
                liability.AugmentedProfits,
                liability.AssociatedCompanyCount,
                liability.CalculationCode,
                liability.TaxBeforeMarginalRelief,
                liability.MarginalRelief,
                period.CorporationTaxChargeable.Value,
                period.OtherReliefs.Value,
                period.TaxPayable.Value,
                liability.IsFilingReady,
                review);
        }).ToArray();

        var acceptedTotals = AcceptedAccountsTotals.Read(accounts.FullAccountsBytes,
            accounts.PeriodStart, accounts.PeriodEnd);
        var currentAccountingProfit = periods.Sum(value => value.AccountingProfit);
        var currentTaxCharge = periods.Sum(value => value.CorporationTaxChargeable);
        var accountingDifference = currentAccountingProfit - acceptedTotals.AccountingProfit;
        var taxDifference = currentTaxCharge - acceptedTotals.TaxCharge;
        var reconciled = Math.Abs(accountingDifference) <= ReconciliationTolerance
            && Math.Abs(taxDifference) <= ReconciliationTolerance;
        var reconciliation = new CorporationTaxAccountsReconciliation(
            acceptedTotals.AccountingProfit, currentAccountingProfit, accountingDifference,
            acceptedTotals.TaxCharge, currentTaxCharge, taxDifference,
            ReconciliationTolerance, reconciled);

        return new(accounts.AccountsSnapshotSha256, sourceSha256, reconciliation, periods,
            accepted.AcceptanceSimulated,
            reconciled && periods.All(value => value.IsFilingReady
                && value.Review is { IsReviewed: true, IsStale: false }));
    }

    public async Task<CorporationTaxCalculationReview> SavePeriodAsync(
        ClaimsPrincipal principal,
        TaxHubCorporationTaxContinuation continuation,
        string logicalFilingIdentity,
        CorporationTaxPeriodReviewedInput input,
        int expectedRevision,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(continuation);
        ArgumentNullException.ThrowIfNull(input);
        if (principal.Identity?.IsAuthenticated != true
            || !(principal.IsInRole(Constants.AdministratorsRole)
                || principal.IsInRole(Constants.ManagersRole)))
            throw new UnauthorizedAccessException(
                "Corporation Tax review requires Administrator or Manager permission.");
        if (expectedRevision < 0)
            throw new ArgumentOutOfRangeException(nameof(expectedRevision));
        var period = continuation.Periods?.SingleOrDefault(value =>
            string.Equals(value.LogicalIdentity, logicalFilingIdentity, StringComparison.Ordinal))
            ?? throw new InvalidOperationException(
                "The Corporation Tax review period is outside the accepted filing scope.");
        if (input.Period.Start != period.PeriodStart || input.Period.End != period.PeriodEnd)
            throw new InvalidOperationException(
                "The Corporation Tax reviewed input does not match its filing period.");
        Validate(input);

        var current = await BuildAsync(continuation, cancellationToken);
        var currentPeriod = current.Periods.Single(value => value.PeriodStart == period.PeriodStart
            && value.PeriodEnd == period.PeriodEnd);
        if (currentPeriod.Review.Revision != expectedRevision)
            throw new InvalidOperationException(
                "The Corporation Tax review changed in another session. Reload it before saving.");

        // Prove the proposed values against the accepted accounts before making the attributable
        // review durable. A failed post-save rebuild would otherwise strand the correction UI behind
        // an invalid persisted review.
        var proposed = await BuildAsync(continuation,
            new(logicalFilingIdentity, input, expectedRevision), cancellationToken);
        var proposedPeriod = proposed.Periods.Single(value =>
            value.PeriodStart == period.PeriodStart && value.PeriodEnd == period.PeriodEnd);
        var proposedInputSha256 = InputSha256(input);
        if (proposedPeriod.Review.Revision != expectedRevision + 1
            || !string.Equals(proposedPeriod.Review.InputSha256, proposedInputSha256,
                StringComparison.Ordinal))
            throw new InvalidOperationException(
                "The Corporation Tax review was not saved because the proposed input could not be verified.");
        if (!proposed.IsReady)
            throw new InvalidOperationException(
                "The Corporation Tax review was not saved because the proposed values do not reconcile to the accepted accounts or are not filing-ready.");

        var identity = await identityAccessor.GetRequiredAsync(principal, cancellationToken);
        var saved = new CorporationTaxReviewedPeriodRecord(
            identity.TenantReference,
            logicalFilingIdentity,
            continuation.AccountsSnapshotSha256,
            period.PeriodStart,
            period.PeriodEnd,
            input,
            proposed.SourceSha256,
            proposedInputSha256,
            expectedRevision + 1,
            identity.ActorReference,
            timeProvider.GetUtcNow());
        if (!await reviewStore.TrySaveAsync(saved, expectedRevision, cancellationToken))
            throw new InvalidOperationException(
                "The Corporation Tax review changed in another session. Reload it before saving.");
        return await BuildAsync(continuation, cancellationToken);
    }

    private static string InputSha256(CorporationTaxPeriodReviewedInput input)
    {
        static string Adjustment(TaxAdjustmentDraft value) =>
            $"{value.Description.Length}:{value.Description}|{value.Amount.ToString(CultureInfo.InvariantCulture)}";
        var loans = input.LoansToParticipators is null
            ? "-"
            : string.Join('|',
                input.LoansToParticipators.LoansOutstandingAtPeriodEnd.ToString(CultureInfo.InvariantCulture),
                input.LoansToParticipators.TaxChargeable.ToString(CultureInfo.InvariantCulture),
                input.LoansToParticipators.TaxPaid.ToString(CultureInfo.InvariantCulture));
        var canonical = string.Join("\n",
            input.Period.Start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            input.Period.End.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            string.Join(";", input.OtherAddBacks.Select(Adjustment)),
            string.Join(";", input.Deductions.Select(Adjustment)),
            input.CapitalAllowances.WritingDownAllowance.ToString(CultureInfo.InvariantCulture),
            input.CapitalAllowances.AnnualInvestmentAllowance.ToString(CultureInfo.InvariantCulture),
            input.CapitalAllowances.OtherAllowances.ToString(CultureInfo.InvariantCulture),
            input.LossesUsed.ToString(CultureInfo.InvariantCulture),
            input.ChargeableGains.ToString(CultureInfo.InvariantCulture),
            input.OtherReliefs.ToString(CultureInfo.InvariantCulture),
            loans);
        return CorporationTaxEvidenceDigests.Hash(canonical);
    }

    private static void Validate(CorporationTaxPeriodReviewedInput input)
    {
        if (input.OtherAddBacks.Any(value => string.IsNullOrWhiteSpace(value.Description)
                || value.Description.Length > 200 || value.Amount < 0m)
            || input.Deductions.Any(value => string.IsNullOrWhiteSpace(value.Description)
                || value.Description.Length > 200 || value.Amount < 0m))
            throw new InvalidOperationException(
                "Each Corporation Tax adjustment requires a description and a non-negative amount.");
        if (input.CapitalAllowances.WritingDownAllowance < 0m
            || input.CapitalAllowances.AnnualInvestmentAllowance < 0m
            || input.CapitalAllowances.OtherAllowances < 0m
            || input.LossesUsed < 0m
            || input.OtherReliefs < 0m
            || input.LoansToParticipators is { } loans
                && (loans.LoansOutstandingAtPeriodEnd < 0m
                    || loans.TaxChargeable < 0m
                    || loans.TaxPaid < 0m))
            throw new InvalidOperationException(
                "Corporation Tax allowances, losses and reliefs cannot be negative.");
    }

    private sealed record AcceptedAccountsTotals(decimal AccountingProfit, decimal TaxCharge)
    {
        private static readonly XNamespace Ix = "http://www.xbrl.org/2013/inlineXBRL";
        private static readonly XNamespace Xbrli = "http://www.xbrl.org/2003/instance";

        internal static AcceptedAccountsTotals Read(byte[] content, DateOnly periodStart,
            DateOnly periodEnd)
        {
            var document = XDocument.Parse(Encoding.UTF8.GetString(content),
                LoadOptions.PreserveWhitespace);
            var contextIds = document.Descendants(Xbrli + "context")
                .Where(context =>
                    string.Equals(context.Descendants(Xbrli + "startDate").SingleOrDefault()?.Value,
                        periodStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        StringComparison.Ordinal)
                    && string.Equals(context.Descendants(Xbrli + "endDate").SingleOrDefault()?.Value,
                        periodEnd.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        StringComparison.Ordinal))
                .Select(context => (string?)context.Attribute("id"))
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToHashSet(StringComparer.Ordinal);
            if (contextIds.Count == 0)
                throw new InvalidOperationException(
                    "The retained full accounts do not contain the accepted accounting period context.");

            decimal Fact(string localName)
            {
                var values = document.Descendants(Ix + "nonFraction")
                    .Where(fact => contextIds.Contains((string?)fact.Attribute("contextRef")))
                    .Where(fact => string.Equals(LocalName((string?)fact.Attribute("name")),
                        localName, StringComparison.Ordinal))
                    .Select(fact => decimal.Parse(fact.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture))
                    .Distinct()
                    .ToArray();
                return values.Length == 1
                    ? values[0]
                    : throw new InvalidOperationException(
                        $"The retained full accounts do not contain one unambiguous {localName} fact for the accepted period.");
            }

            var accountingProfit = Fact("TurnoverRevenue") + Fact("OtherOperatingIncome")
                - Fact("CostSales") - Fact("AdministrativeExpenses");
            var taxCharge = Fact("TaxTaxCreditOnProfitOrLossOnOrdinaryActivities");
            var profitAfterTax = Fact("ProfitLoss");
            if (Math.Abs(accountingProfit - taxCharge - profitAfterTax) > ReconciliationTolerance)
                throw new InvalidOperationException(
                    "The retained full accounts income statement does not reconcile internally.");
            return new(accountingProfit, taxCharge);
        }

        private static string LocalName(string? qualifiedName)
        {
            if (string.IsNullOrWhiteSpace(qualifiedName)) return string.Empty;
            var separator = qualifiedName.IndexOf(':');
            return separator < 0 ? qualifiedName : qualifiedName[(separator + 1)..];
        }
    }
}

internal sealed class DisabledCorporationTaxCalculationReviewService
    : ICorporationTaxCalculationReviewService
{
    public Task<CorporationTaxCalculationReview> BuildAsync(
        TaxHubCorporationTaxContinuation continuation,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(
            "Corporation Tax calculation review is unavailable on this host.");

    public Task<CorporationTaxCalculationReview> SavePeriodAsync(
        ClaimsPrincipal principal,
        TaxHubCorporationTaxContinuation continuation,
        string logicalFilingIdentity,
        CorporationTaxPeriodReviewedInput input,
        int expectedRevision,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(
            "Corporation Tax calculation review is unavailable on this host.");
}
