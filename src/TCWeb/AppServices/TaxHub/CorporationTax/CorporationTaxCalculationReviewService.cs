using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
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

internal sealed record CorporationTaxPeriodCalculationReview(
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
    bool IsFilingReady);

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
}

/// <summary>
/// Recalculates a non-filing CT comparison from current Trade Control transactions and compares it
/// with the exact retained full-accounts artifact. It does not prepare or mutate a filing artifact.
/// </summary>
internal sealed class CorporationTaxCalculationReviewService(
    NodeContext nodeContext,
    ICompaniesHouseAccountsSnapshotService accountsSnapshots,
    IOptions<CompaniesHouseProductHostOptions> hostOptions) : ICorporationTaxCalculationReviewService
{
    private const decimal ReconciliationTolerance = 0.10m;

    public async Task<CorporationTaxCalculationReview> BuildAsync(
        TaxHubCorporationTaxContinuation continuation,
        CancellationToken cancellationToken = default)
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
        foreach (var period in filingPeriods)
        {
            var liability = await taxReader.ReadCorporationTaxLiabilityAsync(connectionString,
                period.PeriodStart.ToDateTime(TimeOnly.MinValue),
                period.PeriodEnd.ToDateTime(TimeOnly.MinValue), cancellationToken);
            reviewedPeriods.Add(new(
                new(period.PeriodStart, period.PeriodEnd),
                [], [], new(0m, 0m, 0m), liability.LossUsed, 0m, 0m, null));
        }

        var source = await new TcCorporationTaxSourceReader(factory, connectionString).ReadAsync(new(
            continuation.AccountsPeriodEnd,
            continuation.AccountsPeriodEnd,
            new(continuation.AccountsPeriodStart, continuation.AccountsPeriodEnd),
            new(null, string.Empty, reviewedPeriods, string.Empty,
                continuation.AccountsPeriodEnd)), cancellationToken);

        var periods = source.CorporationTaxPeriods.Select(period =>
        {
            var liability = period.Liability.Value;
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
                liability.IsFilingReady);
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

        var sourceCanonical = string.Join("\n", source.Versions
            .OrderBy(value => value.SourceCode, StringComparer.Ordinal)
            .ThenBy(value => value.RowVersion, StringComparer.Ordinal)
            .Select(value => $"{value.SourceCode}|{value.RowVersion}|{value.UpdatedOn:O}"));
        var sourceSha256 = Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(sourceCanonical)));
        return new(accounts.AccountsSnapshotSha256, sourceSha256, reconciliation, periods,
            accepted.AcceptanceSimulated,
            reconciled && periods.All(value => value.IsFilingReady));
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
}
