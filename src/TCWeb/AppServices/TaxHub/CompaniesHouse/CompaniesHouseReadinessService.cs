using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeControl.Tax.UK.Adapters.TradeControl.Data;
using TradeControl.Tax.UK.Adapters.TradeControl.Readers;
using TradeControl.Tax.UK.Application.DataProvision;
using TradeControl.Web.Data;
using TradeControl.Web.Pages.Tax.Hub.Models;

namespace TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

public interface ICompaniesHouseReadinessService
{
    Task<TaxHubCompaniesHouseReadiness> AssessAsync(short yearNumber, DateTime selectedPeriodStart,
        CancellationToken cancellationToken = default);
}

public sealed class CompaniesHouseReadinessService(NodeContext nodeContext, TimeProvider timeProvider)
    : ICompaniesHouseReadinessService
{
    private const decimal EquityBridgeTolerance = 0.10m;

    public async Task<TaxHubCompaniesHouseReadiness> AssessAsync(short yearNumber,
        DateTime selectedPeriodStart, CancellationToken cancellationToken = default)
    {
        var periods = await nodeContext.App_tbYearPeriods
            .AsNoTracking()
            .Where(period => period.YearNumber == yearNumber)
            .OrderBy(period => period.StartOn)
            .Select(period => new { period.MonthNumber, period.StartOn })
            .ToListAsync(cancellationToken);
        var year = await nodeContext.App_tbYears.AsNoTracking()
            .Where(item => item.YearNumber == yearNumber)
            .Select(item => new { item.CashStatusCode })
            .SingleOrDefaultAsync(cancellationToken);

        if (periods.Count == 0 || year is null)
            return Unavailable("CH-PERIOD-NOT-FOUND", "The selected accounting year could not be verified.");

        var (firstPeriod, lastPeriod, periodEndDateTime) = ResolvePeriodBounds(
            periods.Select(period => period.StartOn));
        var periodEnd = DateOnly.FromDateTime(periodEndDateTime);
        var isSelectedYearEnd = selectedPeriodStart.Date == lastPeriod;
        var isClosed = year.CashStatusCode == 2;
        var priorClosedYearExists = await nodeContext.App_tbYears.AsNoTracking()
            .AnyAsync(item => item.YearNumber < yearNumber && item.CashStatusCode == 2, cancellationToken);
        var equityVariance = await nodeContext.Cash_vwEquityReconciliationByYears.AsNoTracking()
            .Where(item => item.YearNumber == yearNumber)
            .Select(item => (decimal?)item.Variance)
            .SingleOrDefaultAsync(cancellationToken);

        StatutoryContextSnapshot context;
        try
        {
            var connectionString = nodeContext.Database.GetConnectionString();
            if (string.IsNullOrWhiteSpace(connectionString))
                return Unavailable("CH-SOURCE-CONNECTION-MISSING",
                    "The statutory accounting source is not configured.");
            var contextAsOfDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
            context = await new TcStatutoryContextReader(new ConnectionFactory(), connectionString)
                .ReadAsync(contextAsOfDate, cancellationToken);
        }
        catch
        {
            return Unavailable("CH-STATUTORY-CONTEXT-UNAVAILABLE",
                "The statutory company context could not be verified.");
        }

        var findings = new List<TaxHubCompaniesHouseReadinessFinding>();
        void Block(string code, string message) => findings.Add(new() { IsBlocking = true, Code = code, Message = message });

        if (!isSelectedYearEnd)
            Block("CH-YEAR-END-REQUIRED", "Select the final period of the accounting year for a Companies House filing review.");
        if (!isClosed)
            Block("CH-YEAR-NOT-CLOSED", "The selected accounting year is not closed.");
        if (context.Identity.BusinessTaxTypeCode != 0)
            Block("CH-COMPANY-REQUIRED", "Companies House accounts are available only for a company node.");
        if (string.IsNullOrWhiteSpace(context.Identity.CompanyNumber))
            Block("CH-COMPANY-NUMBER-MISSING", "The company number is missing from the statutory company identity.");

        var profile = context.Profiles.FirstOrDefault(item =>
            item.ReportingTypeCode == "STATUTORY-ACCOUNTS"
            && item.AuthorityCode == "COMPANIES-HOUSE");
        if (profile is null)
            Block("CH-PROFILE-MISSING", "The Companies House statutory-accounts reporting profile is missing.");
        else if (!profile.IsReviewed)
            Block("CH-PROFILE-UNREVIEWED", "The Companies House statutory-accounts reporting profile has not been reviewed.");

        foreach (var finding in StatutoryContextVerifier.Verify(context))
            Block($"CH-{finding.Code}", finding.Message);

        if (!equityVariance.HasValue)
            Block("CH-EQUITY-BRIDGE-MISSING", "The selected year has no Equity Bridge evidence.");
        else if (Math.Abs(equityVariance.Value) > EquityBridgeTolerance)
            Block("CH-EQUITY-BRIDGE-FAILED", "The selected year's Equity Bridge exceeds the accepted tolerance.");

        var isEligible = findings.All(item => !item.IsBlocking);
        return new()
        {
            IsEligible = isEligible,
            Status = isEligible ? "Ready to prepare document" : "Not ready",
            CompanyNumberDisplay = MaskCompanyNumber(context.Identity.CompanyNumber),
            PeriodDisplay = $"{firstPeriod:dd MMM yyyy} – {periodEndDateTime:dd MMM yyyy}",
            AccountsSequence = priorClosedYearExists ? "Subsequent accounts with comparatives" : "First accounts",
            EquityBridgeStatus = equityVariance.HasValue
                ? $"{(Math.Abs(equityVariance.Value) <= EquityBridgeTolerance ? "Pass" : "Fail")} · variance {equityVariance.Value:0.00}"
                : "Missing",
            IsYearClosed = isClosed,
            IsExactDocumentPrepared = false,
            ExternalRequestMade = false,
            Findings = findings
        };
    }

    private static TaxHubCompaniesHouseReadiness Unavailable(string code, string message) => new()
    {
        Status = "Unavailable",
        Findings = [new() { IsBlocking = true, Code = code, Message = message }],
        IsExactDocumentPrepared = false,
        ExternalRequestMade = false
    };

    internal static string MaskCompanyNumber(string? value)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length <= 4)
            return normalized;
        return $"{new string('•', normalized.Length - 4)}{normalized[^4..]}";
    }

    internal static (DateTime PeriodStart, DateTime LastPeriodStart, DateTime PeriodEnd)
        ResolvePeriodBounds(IEnumerable<DateTime> periodStarts)
    {
        var ordered = periodStarts.Select(value => value.Date).OrderBy(value => value).ToArray();
        if (ordered.Length == 0)
            throw new ArgumentException("At least one accounting period is required.", nameof(periodStarts));
        var last = ordered[^1];
        return (ordered[0], last,
            new DateTime(last.Year, last.Month, DateTime.DaysInMonth(last.Year, last.Month)));
    }
}
