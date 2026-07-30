using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;
using TradeControl.Web.Pages.Tax.Hub.Models;

namespace TradeControl.Web.AppServices.TaxHub
{
    public sealed class TaxHubService : ITaxHubService
    {
        private const string CapitalCode = "CAPITAL";
        private const decimal ValidationTolerance = 0.10m;

        private readonly NodeContext _nodeContext;

        public TaxHubService(NodeContext nodeContext)
        {
            _nodeContext = nodeContext;
        }

        public Task<TaxHubResult> GetShellStateAsync()
        {
            var result = new TaxHubResult
            {
                IsSuccess = true,
                Message = "Tax Hub is ready.",
                State = new TaxHubWorkflowState
                {
                    SelectedWorkspace = TaxHubWorkspace.Dashboard,
                    Title = "Tax Hub",
                    Subtitle = "Reporting and validation workspace"
                }
            };

            return Task.FromResult(result);
        }

        public async Task<TaxHubDashboardModel> GetDashboardAsync()
        {
            NodeSettings nodeSettings = new NodeSettings(_nodeContext);
            var bizTaxType = await nodeSettings.BizTaxType();

            var taxTypes = await _nodeContext.App_TaxTypes
                .AsNoTracking()
                .Where(t => t.TaxTypeCode == (short)bizTaxType || t.TaxTypeCode == (short)NodeEnum.TaxType.VAT)
                .OrderBy(t => t.TaxTypeCode)
                .ToListAsync();

            var obligations = new List<TaxHubObligationSummary>();

            foreach (var taxType in taxTypes)
            {
                var nextFiling = await GetNextDueDateAsync(taxType.TaxTypeCode, isAccrual: true);
                var nextPayment = await GetNextDueDateAsync(taxType.TaxTypeCode, isAccrual: false);

                obligations.Add(new TaxHubObligationSummary
                {
                    TaxTypeCode = taxType.TaxTypeCode,
                    TaxType = taxType.TaxType,
                    CashCode = taxType.CashCode,
                    CashDescription = taxType.CashDescription,
                    SubjectCode = taxType.SubjectCode,
                    SubjectName = taxType.SubjectName,
                    NextFilingDueOn = nextFiling?.PayOn,
                    NextPaymentDueOn = nextPayment?.PayOn,
                    FilingPeriodFrom = nextFiling?.PayFrom,
                    FilingPeriodTo = nextFiling?.PayTo,
                    PaymentPeriodFrom = nextPayment?.PayFrom,
                    PaymentPeriodTo = nextPayment?.PayTo
                });
            }

            var vatTotals = await _nodeContext.Cash_TaxVatTotals
                .AsNoTracking()
                .OrderByDescending(t => t.StartOn)
                .FirstOrDefaultAsync();

            var businessTaxTotals = await _nodeContext.Cash_TaxBizTotals
                .AsNoTracking()
                .Where(t => t.StartOn <= DateTime.Today)
                .OrderByDescending(t => t.StartOn)
                .FirstOrDefaultAsync();

            var projectedDue = await _nodeContext.GetProjectedTaxDueAsync();

            var businessType = await _nodeContext.Cash_tbTaxTypes
                .AsNoTracking()
                .Where(t => t.TaxTypeCode == (short)bizTaxType)
                .Select(t => t.TaxTypeCode == 0 ? "Limited Company" : "Sole Trader")
                .FirstOrDefaultAsync() ?? string.Empty;

            var validationSummary = await GetAccountsValidationSummaryAsync();
            var businessTaxCard = await BuildBusinessTaxCard(businessTaxTotals, obligations, projectedDue);
            var payloadAudit = await GetPayloadAuditSummaryAsync();

            return new TaxHubDashboardModel
            {
                BusinessType = businessType,
                ActiveRegimes = taxTypes
                    .Select(t => t.TaxType)
                    .ToArray(),
                Obligations = obligations,
                PayloadAudit = payloadAudit,
                Cards = new[]
                {
                    BuildVatCard(vatTotals, obligations, projectedDue),
                    businessTaxCard,
                    BuildAccountsCard(validationSummary)
                }
            };
        }

        public async Task<IReadOnlyList<TaxHubYearOption>> GetYearsAsync()
        {
            return await _nodeContext.App_tbYears
                .AsNoTracking()
                .Where(t => t.CashStatusCode > 0 && t.CashStatusCode < 3)
                .OrderByDescending(t => t.YearNumber)
                .Select(t => new TaxHubYearOption
                {
                    YearNumber = t.YearNumber,
                    Description = t.Description
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<TaxHubPeriodOption>> GetPeriodsAsync(short yearNumber)
        {
            return await _nodeContext.App_Periods
                .AsNoTracking()
                .Where(t => t.YearNumber == yearNumber)
                .OrderByDescending(t => t.StartOn)
                .Select(t => new TaxHubPeriodOption
                {
                    YearNumber = t.YearNumber,
                    MonthNumber = t.MonthNumber,
                    StartOn = t.StartOn,
                    Description = t.Description
                })
                .ToListAsync();
        }

        public async Task<TaxHubPeriodOption?> GetDefaultPeriodAsync()
        {
            var activeStartOn = new FinancialPeriods(_nodeContext).ActiveStartOn;

            return await _nodeContext.App_Periods
                .AsNoTracking()
                .Where(t => t.StartOn == activeStartOn)
                .Select(t => new TaxHubPeriodOption
                {
                    YearNumber = t.YearNumber,
                    MonthNumber = t.MonthNumber,
                    StartOn = t.StartOn,
                    Description = t.Description
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TaxHubVatWorkspaceModel> GetVatWorkspaceAsync(short? yearNumber, DateTime? periodStartOn)
        {
            var totalsQuery = _nodeContext.Cash_TaxVatTotals
                .AsNoTracking()
                .AsQueryable();

            if (yearNumber.HasValue)
                totalsQuery = totalsQuery.Where(t => t.YearNumber == yearNumber.Value);

            var totals = await totalsQuery
                .OrderByDescending(t => t.StartOn)
                .Select(t => new TaxHubVatTotalRow
                {
                    YearNumber = t.YearNumber,
                    Description = t.Description,
                    Period = t.Period,
                    StartOn = t.StartOn,
                    vatDueSales = t.vatDueSales,
                    vatDueAcquisitions = t.vatDueAcquisitions,
                    totalVatDue = t.totalVatDue,
                    vatReclaimedCurrPeriod = t.vatReclaimedCurrPeriod,
                    netVatDue = t.netVatDue,
                    totalValueSalesExVAT = t.totalValueSalesExVAT,
                    totalValuePurchasesExVAT = t.totalValuePurchasesExVAT,
                    totalValueGoodsSuppliedExVAT = t.totalValueGoodsSuppliedExVAT,
                    totalValueGoodsReceivedExVAT = t.totalValueGoodsReceivedExVAT
                })
                .ToListAsync();

            var statement = await _nodeContext.Cash_TaxVatStatement
                .AsNoTracking()
                .OrderByDescending(t => t.RowNumber)
                .Select(t => new TaxHubVatStatementRow
                {
                    RowNumber = t.RowNumber,
                    StartOn = t.StartOn,
                    VatDue = t.VatDue,
                    VatPaid = t.VatPaid,
                    Balance = t.Balance
                })
                .ToListAsync();

            DateTime? selectedStartOn = periodStartOn?.Date;
            bool isAllPeriodsInYear = !selectedStartOn.HasValue;

            if (!yearNumber.HasValue)
                selectedStartOn = null;

            string? selectedPeriodName = null;
            string? selectedYearDescription = null;

            if (yearNumber.HasValue)
            {
                selectedYearDescription = await _nodeContext.App_tbYears
                    .AsNoTracking()
                    .Where(t => t.YearNumber == yearNumber.Value)
                    .Select(t => t.Description)
                    .FirstOrDefaultAsync();
            }

            if (selectedStartOn.HasValue)
            {
                selectedPeriodName = await _nodeContext.App_Periods
                    .AsNoTracking()
                    .Where(t => t.StartOn == selectedStartOn.Value)
                    .Select(t => t.Description)
                    .FirstOrDefaultAsync();
            }

            var periodsQuery = _nodeContext.Cash_TaxVatSummary
                .AsNoTracking()
                .AsQueryable();

            if (selectedStartOn.HasValue)
            {
                periodsQuery = periodsQuery.Where(t => t.StartOn == selectedStartOn.Value);
            }
            else if (yearNumber.HasValue)
            {
                periodsQuery = periodsQuery
                    .Join(
                        _nodeContext.App_Periods.AsNoTracking().Where(p => p.YearNumber == yearNumber.Value),
                        summary => summary.StartOn,
                        period => period.StartOn,
                        (summary, period) => summary);
            }

            var periods = await periodsQuery
                .OrderBy(t => t.StartOn)
                .ThenBy(t => t.TaxCode)
                .Select(t => new TaxHubVatPeriodRow
                {
                    StartOn = t.StartOn,
                    TaxCode = t.TaxCode,
                    vatDueSales = t.vatDueSales,
                    vatDueAcquisitions = t.vatDueAcquisitions,
                    vatReclaimedCurrPeriod = t.vatReclaimedCurrPeriod,
                    totalValueSalesExVAT = t.totalValueSalesExVAT,
                    totalValuePurchasesExVAT = t.totalValuePurchasesExVAT,
                    totalValueGoodsSuppliedExVAT = t.totalValueGoodsSuppliedExVAT,
                    totalValueGoodsReceivedExVAT = t.totalValueGoodsReceivedExVAT
                })
                .ToListAsync();

            var activeStatementStartOn = statement
                .Where(t => t.StartOn.Date >= DateTime.Today)
                .OrderBy(t => t.StartOn)
                .Select(t => (DateTime?)t.StartOn)
                .FirstOrDefault();

            return new TaxHubVatWorkspaceModel
            {
                SelectedPeriodName = selectedPeriodName,
                SelectedPeriodStartOn = selectedStartOn,
                SelectedYearNumber = yearNumber,
                SelectedYearDescription = selectedYearDescription,
                IsAllYears = !yearNumber.HasValue,
                IsAllPeriodsInYear = isAllPeriodsInYear,
                ActiveStatementStartOn = activeStatementStartOn,
                Totals = totals,
                Statement = statement,
                Periods = periods
            };
        }

        public async Task<TaxHubBusinessTaxWorkspaceModel> GetBusinessTaxWorkspaceAsync(short? yearNumber, DateTime? periodStartOn)
        {
            var totalsQuery = _nodeContext.Cash_TaxBizTotals
                .AsNoTracking()
                .Where(t => t.StartOn <= DateTime.Today)
                .AsQueryable();

            if (yearNumber.HasValue)
                totalsQuery = totalsQuery.Where(t => t.YearNumber == yearNumber.Value);

            var totals = await totalsQuery
                .OrderByDescending(t => t.StartOn)
                .Select(t => new TaxHubBusinessTaxTotalRow
                {
                    YearNumber = t.YearNumber,
                    StartOn = t.StartOn,
                    Description = t.Description,
                    Period = t.Period,
                    BusinessTaxRate = t.BusinessTaxRate,
                    BusinessTaxAdjustment = t.BusinessTaxAdjustment,
                    NetProfit = t.NetProfit,
                    BusinessTax = t.BusinessTax
                })
                .ToListAsync();

            var statement = await _nodeContext.Cash_TaxBizStatement
                .AsNoTracking()
                .OrderBy(t => t.StartOn)
                .Select(t => new TaxHubBusinessTaxStatementRow
                {
                    StartOn = t.StartOn,
                    TaxDue = t.TaxDue,
                    TaxPaid = t.TaxPaid,
                    Balance = t.Balance
                })
                .ToListAsync();

            var lossesCarriedForward = await _nodeContext.Cash_TaxLossesCarriedForward
                .AsNoTracking()
                .OrderBy(t => t.StartOn)
                .Select(t => new TaxHubBusinessTaxLossesRow
                {
                    YearEndDescription = t.YearEndDescription,
                    StartOn = t.StartOn,
                    TaxDue = t.TaxDue,
                    TaxBalance = t.TaxBalance,
                    LossesCarriedForward = t.LossesCarriedForward
                })
                .ToListAsync();

            string? selectedYearDescription = null;
            string? selectedPeriodDescription = null;
            bool isAllPeriodsInYear = !periodStartOn.HasValue;

            if (yearNumber.HasValue)
            {
                selectedYearDescription = await _nodeContext.App_tbYears
                    .AsNoTracking()
                    .Where(t => t.YearNumber == yearNumber.Value)
                    .Select(t => t.Description)
                    .FirstOrDefaultAsync();
            }

            if (periodStartOn.HasValue)
            {
                selectedPeriodDescription = await _nodeContext.App_Periods
                    .AsNoTracking()
                    .Where(t => t.StartOn == periodStartOn.Value)
                    .Select(t => t.Description)
                    .FirstOrDefaultAsync();
            }

            var sourceNames = await _nodeContext.Cash_tbTaxTagSources
                .AsNoTracking()
                .ToDictionaryAsync(t => t.TaxSourceCode, t => t.SourceName);

            var yearPeriodStartDates = yearNumber.HasValue
                ? await _nodeContext.App_tbYearPeriods
                    .AsNoTracking()
                    .Where(t => t.YearNumber == yearNumber.Value)
                    .OrderBy(t => t.StartOn)
                    .Select(t => t.StartOn)
                    .ToListAsync()
                : new List<DateTime>();

            DateTime? yearStart = yearPeriodStartDates.Count > 0 ? yearPeriodStartDates.First() : null;
            DateTime? yearLastPeriodStart = yearPeriodStartDates.Count > 0 ? yearPeriodStartDates.Last() : null;
            DateTime? yearEndExclusive = null;

            if (yearLastPeriodStart.HasValue)
            {
                var nextPeriodStart = await _nodeContext.App_tbYearPeriods
                    .AsNoTracking()
                    .Where(t => t.StartOn > yearLastPeriodStart.Value)
                    .OrderBy(t => t.StartOn)
                    .Select(t => (DateTime?)t.StartOn)
                    .FirstOrDefaultAsync();

                yearEndExclusive = nextPeriodStart ?? yearLastPeriodStart.Value.AddMonths(1);
            }

            var payloadQuery = _nodeContext.Cash_vwTaxBizPayloads
                .AsNoTracking()
                .AsQueryable();

            if (periodStartOn.HasValue)
            {
                payloadQuery = payloadQuery.Where(t => t.PeriodStartOn == periodStartOn.Value);
            }
            else if (yearPeriodStartDates.Count > 0)
            {
                payloadQuery = payloadQuery.Where(t => yearPeriodStartDates.Contains(t.PeriodStartOn));
            }

            var payload = await payloadQuery
                .OrderBy(t => t.TaxSourceCode)
                .ThenBy(t => t.PeriodFrom)
                .ThenBy(t => t.TagCode)
                .ThenBy(t => t.CashCode)
                .Select(t => new TaxHubBusinessTaxPayloadRow
                {
                    TaxSourceCode = t.TaxSourceCode,
                    TagCode = t.TagCode,
                    ParentCode = t.ParentCode,
                    CashCode = t.CashCode,
                    CategoryCode = t.CategoryCode,
                    CashTypeCode = t.CashTypeCode,
                    PeriodStartOn = t.PeriodStartOn,
                    PeriodFrom = t.PeriodFrom,
                    PeriodTo = t.PeriodTo,
                    PeriodInvoiceValue = t.PeriodInvoiceValue
                })
                .ToListAsync();

            var submissionQuery = _nodeContext.Cash_vwTaxBizSubmissions
                .AsNoTracking()
                .AsQueryable();

            if (yearStart.HasValue && yearEndExclusive.HasValue)
            {
                submissionQuery = submissionQuery.Where(t =>
                    t.PeriodFrom < yearEndExclusive.Value &&
                    t.PeriodTo >= yearStart.Value);
            }

            if (periodStartOn.HasValue)
            {
                submissionQuery = submissionQuery.Where(t =>
                    t.PeriodFrom <= periodStartOn.Value &&
                    t.PeriodTo >= periodStartOn.Value);
            }

            var submissions = await submissionQuery
                .OrderBy(t => t.TaxSourceCode)
                .ThenBy(t => t.PeriodFrom)
                .ThenBy(t => t.TagCode)
                .Select(t => new TaxHubBusinessTaxSubmissionRow
                {
                    TaxSourceCode = t.TaxSourceCode,
                    TagCode = t.TagCode,
                    PeriodFrom = t.PeriodFrom,
                    PeriodTo = t.PeriodTo,
                    TaxableAmount = t.TaxableAmount
                })
                .ToListAsync();

            var taxSourceCodes = payload
                .Select(t => t.TaxSourceCode)
                .Concat(submissions.Select(t => t.TaxSourceCode))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(t => t)
                .ToList();

            var sources = taxSourceCodes
                .Select(taxSourceCode => new TaxHubBusinessTaxSourceWorkspace
                {
                    TaxSourceCode = taxSourceCode,
                    SourceName = sourceNames.TryGetValue(taxSourceCode, out var sourceName) ? sourceName : taxSourceCode,
                    Submissions = submissions
                        .Where(t => string.Equals(t.TaxSourceCode, taxSourceCode, StringComparison.OrdinalIgnoreCase))
                        .ToList(),
                    Payload = payload
                        .Where(t => string.Equals(t.TaxSourceCode, taxSourceCode, StringComparison.OrdinalIgnoreCase))
                        .ToList()
                })
                .ToList();

            return new TaxHubBusinessTaxWorkspaceModel
            {
                SelectedYearNumber = yearNumber,
                SelectedYearDescription = selectedYearDescription,
                SelectedPeriodStartOn = periodStartOn,
                SelectedPeriodDescription = selectedPeriodDescription,
                IsAllYears = !yearNumber.HasValue,
                IsAllPeriodsInYear = isAllPeriodsInYear,
                Totals = totals,
                Statement = statement,
                LossesCarriedForward = lossesCarriedForward,
                Sources = sources
            };
        }

        public async Task<TaxHubAccountsWorkspaceModel> GetAccountsWorkspaceAsync(short? yearNumber, DateTime? periodStartOn)
        {
            var financialPeriods = new FinancialPeriods(_nodeContext);
            var selectedYear = yearNumber ?? financialPeriods.ActiveYear;
            var selectedPeriod = periodStartOn ?? financialPeriods.ActiveStartOn;

            var selectedYearName = await _nodeContext.App_tbYears
                .AsNoTracking()
                .Where(t => t.YearNumber == selectedYear)
                .Select(t => t.Description)
                .FirstOrDefaultAsync() ?? string.Empty;

            var selectedPreviousYearNumber = await _nodeContext.Cash_ProfitAndLossByYear
                .AsNoTracking()
                .Where(t => t.YearNumber < selectedYear)
                .MaxAsync(t => (short?)t.YearNumber);

            var selectedPreviousYearName = selectedPreviousYearNumber.HasValue
                ? await _nodeContext.App_tbYears
                    .AsNoTracking()
                    .Where(t => t.YearNumber == selectedPreviousYearNumber.Value)
                    .Select(t => t.Description)
                    .FirstOrDefaultAsync() ?? string.Empty
                : string.Empty;

            var annualTrade = await _nodeContext.Cash_ProfitAndLossByYear
                .AsNoTracking()
                .Where(t => t.CashTypeCode == (short)NodeEnum.CashType.Trade)
                .OrderByDescending(t => t.YearNumber)
                .ThenBy(t => t.DisplayOrder)
                .ToListAsync();

            var annualTax = await _nodeContext.Cash_ProfitAndLossByYear
                .AsNoTracking()
                .Where(t => t.CashTypeCode == (short)NodeEnum.CashType.External)
                .OrderByDescending(t => t.YearNumber)
                .ThenBy(t => t.DisplayOrder)
                .ToListAsync();

            var annualProfitAndLoss = BuildProfitAndLossRows(
                annualTrade.Where(t => t.YearNumber == selectedYear),
                selectedPreviousYearNumber.HasValue
                    ? annualTrade.Where(t => t.YearNumber == selectedPreviousYearNumber.Value)
                    : Enumerable.Empty<Cash_vwProfitAndLossByYear>());

            var annualTaxTotals = BuildProfitAndLossRows(
                annualTax.Where(t => t.YearNumber == selectedYear),
                selectedPreviousYearNumber.HasValue
                    ? annualTax.Where(t => t.YearNumber == selectedPreviousYearNumber.Value)
                    : Enumerable.Empty<Cash_vwProfitAndLossByYear>());

            var annualDetails = await BuildAnnualProfitAndLossDetailsAsync(selectedYear, selectedPreviousYearNumber);

            var selectedPeriodInfo = await _nodeContext.App_tbYearPeriods
                .AsNoTracking()
                .Where(t => t.StartOn == selectedPeriod)
                .FirstOrDefaultAsync();

            var selectedPeriodName = await _nodeContext.App_Periods
                .AsNoTracking()
                .Where(t => t.StartOn == selectedPeriod)
                .Select(t => t.Description)
                .FirstOrDefaultAsync() ?? string.Empty;

            string selectedPreviousPeriodName = string.Empty;
            short? selectedPreviousPeriodYear = null;

            if (selectedPeriodInfo is not null)
            {
                selectedPreviousPeriodYear = await _nodeContext.App_tbYearPeriods
                    .AsNoTracking()
                    .Where(t => t.MonthNumber == selectedPeriodInfo.MonthNumber && t.YearNumber < selectedPeriodInfo.YearNumber)
                    .MaxAsync(t => (short?)t.YearNumber);

                if (selectedPreviousPeriodYear.HasValue)
                {
                    selectedPreviousPeriodName = await _nodeContext.App_Periods
                        .AsNoTracking()
                        .Where(t => t.YearNumber == selectedPreviousPeriodYear.Value && t.MonthNumber == selectedPeriodInfo.MonthNumber)
                        .Select(t => t.Description)
                        .FirstOrDefaultAsync() ?? string.Empty;
                }
            }

            var monthlyTrade = selectedPeriodInfo is null
                ? new List<Cash_vwProfitAndLossByPeriod>()
                : await _nodeContext.Cash_ProfitAndLossByMonth
                    .AsNoTracking()
                    .Where(t =>
                        t.MonthNumber == selectedPeriodInfo.MonthNumber &&
                        t.StartOn <= selectedPeriod &&
                        t.CashTypeCode == (short)NodeEnum.CashType.Trade)
                    .OrderByDescending(t => t.YearNumber)
                    .ThenBy(t => t.DisplayOrder)
                    .ToListAsync();

            var monthlyTax = selectedPeriodInfo is null
                ? new List<Cash_vwProfitAndLossByPeriod>()
                : await _nodeContext.Cash_ProfitAndLossByMonth
                    .AsNoTracking()
                    .Where(t =>
                        t.MonthNumber == selectedPeriodInfo.MonthNumber &&
                        t.StartOn <= selectedPeriod &&
                        t.CashTypeCode == (short)NodeEnum.CashType.External)
                    .OrderByDescending(t => t.YearNumber)
                    .ThenBy(t => t.DisplayOrder)
                    .ToListAsync();

            var monthlyProfitAndLoss = selectedPeriodInfo is null
                ? Array.Empty<TaxHubProfitAndLossRow>()
                : BuildProfitAndLossRows(
                    monthlyTrade.Where(t => t.YearNumber == selectedPeriodInfo.YearNumber),
                    selectedPreviousPeriodYear.HasValue
                        ? monthlyTrade.Where(t => t.YearNumber == selectedPreviousPeriodYear.Value)
                        : Enumerable.Empty<Cash_vwProfitAndLossByPeriod>());

            var monthlyTaxTotals = selectedPeriodInfo is null
                ? Array.Empty<TaxHubProfitAndLossRow>()
                : BuildProfitAndLossRows(
                    monthlyTax.Where(t => t.YearNumber == selectedPeriodInfo.YearNumber),
                    selectedPreviousPeriodYear.HasValue
                        ? monthlyTax.Where(t => t.YearNumber == selectedPreviousPeriodYear.Value)
                        : Enumerable.Empty<Cash_vwProfitAndLossByPeriod>());

            var monthlyDetails = selectedPeriodInfo is null
                ? Array.Empty<TaxHubProfitAndLossDetailSection>()
                : await BuildMonthlyProfitAndLossDetailsAsync(selectedPeriod, selectedPeriodInfo.YearNumber, selectedPreviousPeriodYear, selectedPeriodInfo.MonthNumber);

            var balanceSheet = selectedPeriodInfo is null
                ? Array.Empty<TaxHubBalanceSheetRow>()
                : await BuildBalanceSheetAsync(selectedPeriod, selectedPeriodInfo);

            var isYearEndBalanceSheet = selectedPeriodInfo is not null
                && selectedPeriodInfo.MonthNumber == 12;

            var equityReconciliation = await GetEquityReconciliationRowsAsync();
            var validationSummary = BuildAccountsValidationSummary(equityReconciliation);

            return new TaxHubAccountsWorkspaceModel
            {
                SelectedYearNumber = selectedYear,
                SelectedYearName = selectedYearName,
                SelectedPreviousYearName = selectedPreviousYearName,
                SelectedPeriodStartOn = selectedPeriod,
                SelectedPeriodName = selectedPeriodName,
                SelectedPreviousPeriodName = selectedPreviousPeriodName,
                IsYearEndBalanceSheet = isYearEndBalanceSheet,
                AnnualProfitAndLoss = annualProfitAndLoss,
                AnnualTaxTotals = annualTaxTotals,
                MonthlyProfitAndLoss = monthlyProfitAndLoss,
                MonthlyTaxTotals = monthlyTaxTotals,
                AnnualDetails = annualDetails,
                MonthlyDetails = monthlyDetails,
                BalanceSheet = balanceSheet,
                ValidationSummary = validationSummary,
                EquityReconciliation = equityReconciliation
            };
        }

        private async Task<TaxHubPayloadAuditSummary> GetPayloadAuditSummaryAsync()
        {
            var rows = await _nodeContext.Cash_vwTaxBizPayloadAudits
                .AsNoTracking()
                .Select(t => new
                {
                    t.Difference
                })
                .ToListAsync();

            var errorRows = rows.Count(t => t.Difference != 0m);
            var totalDifference = rows.Sum(t => t.Difference);

            return new TaxHubPayloadAuditSummary
            {
                Status = totalDifference == 0m ? "PASS" : "FAIL",
                TotalRows = rows.Count,
                ErrorRows = errorRows,
                TotalDifference = totalDifference
            };
        }

        private async Task<IReadOnlyList<TaxHubEquityReconciliationRow>> GetEquityReconciliationRowsAsync()
        {
            var rows = await _nodeContext.Cash_vwEquityReconciliationByYears
                .AsNoTracking()
                .OrderBy(t => t.YearNumber)
                .ToListAsync();

            return rows
                .Select(t =>
                {
                    var bridgeTotal = t.ProfitAfterTax
                        + t.CapitalMovement
                        + t.OpeningSubjectPosition
                        + t.OpeningAccountPosition;

                    var variance = t.Variance;
                    var status = GetValidationStatus(variance);

                    return new TaxHubEquityReconciliationRow
                    {
                        YearNumber = t.YearNumber,
                        Description = t.Description ?? string.Empty,
                        OpeningCapital = t.OpeningCapital,
                        ClosingCapital = t.ClosingCapital,
                        Profit = t.Profit,
                        BusinessTax = t.BusinessTax,
                        ProfitAfterTax = t.ProfitAfterTax,
                        TaxCarry = t.TaxCarry,
                        CapitalMovement = t.CapitalMovement,
                        OpeningSubjectPosition = t.OpeningSubjectPosition,
                        OpeningAccountPosition = t.OpeningAccountPosition,
                        OpeningLossesCarriedForward = t.OpeningLossesCarriedForward,
                        ClosingLossesCarriedForward = t.ClosingLossesCarriedForward,
                        LossesCarriedForwardDelta = t.LossesCarriedForwardDelta,
                        CapitalDelta = t.CapitalDelta,
                        Variance = variance,
                        BridgeTotal = bridgeTotal,
                        Status = status
                    };
                })
                .ToList();
        }

        private async Task<TaxHubAccountsValidationSummary> GetAccountsValidationSummaryAsync()
        {
            var rows = await GetEquityReconciliationRowsAsync();
            return BuildAccountsValidationSummary(rows);
        }

        private static TaxHubAccountsValidationSummary BuildAccountsValidationSummary(IReadOnlyList<TaxHubEquityReconciliationRow> rows)
        {
            var passCount = rows.Count(t => t.Status == "PASS");
            var warnCount = rows.Count(t => t.Status == "WARN");
            var failCount = rows.Count(t => t.Status == "FAIL");

            var status = failCount > 0
                ? "FAIL"
                : warnCount > 0
                    ? "WARN"
                    : rows.Count > 0
                        ? "PASS"
                        : "PENDING";

            return new TaxHubAccountsValidationSummary
            {
                Tolerance = ValidationTolerance,
                TotalRows = rows.Count,
                PassCount = passCount,
                WarnCount = warnCount,
                FailCount = failCount,
                Status = status
            };
        }

        private static string GetValidationStatus(decimal variance)
        {
            var absolute = Math.Abs(variance);

            if (absolute <= ValidationTolerance)
                return "PASS";

            if (absolute <= ValidationTolerance * 10)
                return "WARN";

            return "FAIL";
        }

        private async Task<IReadOnlyList<TaxHubBalanceSheetRow>> BuildBalanceSheetAsync(DateTime selectedPeriod, App_tbYearPeriod selectedPeriodInfo)
        {
            var balances = await _nodeContext.Cash_BalanceSheet
                .AsNoTracking()
                .Where(t => t.MonthNumber == selectedPeriodInfo.MonthNumber && t.StartOn <= selectedPeriod)
                .OrderByDescending(t => t.YearNumber)
                .ThenByDescending(t => t.CashPolarityCode)
                .ThenByDescending(t => t.LiquidityLevel)
                .ThenByDescending(t => t.EntryNumber)
                .ToListAsync();

            var currentRows = balances
                .Where(t => t.YearNumber == selectedPeriodInfo.YearNumber)
                .Select(t => new TaxHubBalanceSheetRow
                {
                    AssetCode = t.AssetCode,
                    AssetName = t.AssetName,
                    CurrentBalance = Convert.ToDecimal(t.Balance)
                })
                .ToList();

            var previousYearNumber = balances
                .Where(t => t.YearNumber < selectedPeriodInfo.YearNumber)
                .Select(t => (short?)t.YearNumber)
                .Max();

            if (previousYearNumber.HasValue)
            {
                foreach (var previous in balances.Where(t => t.YearNumber == previousYearNumber.Value))
                {
                    var existing = currentRows.FirstOrDefault(t => t.AssetCode == previous.AssetCode);

                    if (existing is not null)
                    {
                        currentRows[currentRows.IndexOf(existing)] = new TaxHubBalanceSheetRow
                        {
                            AssetCode = existing.AssetCode,
                            AssetName = existing.AssetName,
                            CurrentBalance = existing.CurrentBalance,
                            PreviousBalance = Convert.ToDecimal(previous.Balance)
                        };
                    }
                    else
                    {
                        currentRows.Add(new TaxHubBalanceSheetRow
                        {
                            AssetCode = previous.AssetCode,
                            AssetName = previous.AssetName,
                            CurrentBalance = 0m,
                            PreviousBalance = Convert.ToDecimal(previous.Balance)
                        });
                    }
                }
            }

            currentRows.Add(new TaxHubBalanceSheetRow
            {
                AssetCode = CapitalCode,
                AssetName = CapitalCode,
                CurrentBalance = currentRows.Sum(t => t.CurrentBalance),
                PreviousBalance = currentRows.Sum(t => t.PreviousBalance),
                IsCapital = true
            });

            return currentRows;
        }

        private async Task<IReadOnlyList<TaxHubProfitAndLossDetailSection>> BuildAnnualProfitAndLossDetailsAsync(short currentYearNumber, short? previousYearNumber)
        {
            var categories = await _nodeContext.Cash_FlowCategories
                .AsNoTracking()
                .OrderBy(c => c.EntryId)
                .ToListAsync();

            var cashCodes = await _nodeContext.Cash_tbCodes
                .AsNoTracking()
                .Where(c => c.IsEnabled != 0)
                .Select(c => new { c.CashCode, c.CashDescription })
                .ToDictionaryAsync(c => c.CashCode, c => c.CashDescription);

            var currentRows = await _nodeContext.Cash_FlowCategoryByYears
                .AsNoTracking()
                .Where(c => c.YearNumber == currentYearNumber)
                .ToListAsync();

            var previousRows = previousYearNumber.HasValue
                ? await _nodeContext.Cash_FlowCategoryByYears
                    .AsNoTracking()
                    .Where(c => c.YearNumber == previousYearNumber.Value)
                    .ToListAsync()
                : new List<Cash_vwFlowCategoryByYear>();

            return BuildProfitAndLossDetailSections(
                categories,
                currentRows.Select(r => (
                    r.CategoryCode,
                    r.CashCode,
                    cashCodes.TryGetValue(r.CashCode, out var description) ? description : r.CashCode,
                    r.InvoiceValue ?? 0m)),
                previousRows.Select(r => (
                    r.CategoryCode,
                    r.CashCode,
                    cashCodes.TryGetValue(r.CashCode, out var description) ? description : r.CashCode,
                    r.InvoiceValue ?? 0m)));
        }

        private async Task<IReadOnlyList<TaxHubProfitAndLossDetailSection>> BuildMonthlyProfitAndLossDetailsAsync(
            DateTime selectedPeriod,
            short currentYearNumber,
            short? previousYearNumber,
            short monthNumber)
        {
            var categories = await _nodeContext.Cash_FlowCategories
                .AsNoTracking()
                .OrderBy(c => c.EntryId)
                .ToListAsync();

            var cashCodes = await _nodeContext.Cash_tbCodes
                .AsNoTracking()
                .Where(c => c.IsEnabled != 0)
                .Select(c => new { c.CashCode, c.CashDescription })
                .ToDictionaryAsync(c => c.CashCode, c => c.CashDescription);

            var currentRows = await _nodeContext.Cash_FlowCategoryByPeriods
                .AsNoTracking()
                .Where(c => c.StartOn == selectedPeriod)
                .ToListAsync();

            List<Cash_vwFlowCategoryByPeriod> previousRows;

            if (previousYearNumber.HasValue)
            {
                var previousStartOn = await _nodeContext.App_tbYearPeriods
                    .AsNoTracking()
                    .Where(p => p.YearNumber == previousYearNumber.Value && p.MonthNumber == monthNumber)
                    .Select(p => (DateTime?)p.StartOn)
                    .SingleOrDefaultAsync();

                previousRows = previousStartOn.HasValue
                    ? await _nodeContext.Cash_FlowCategoryByPeriods
                        .AsNoTracking()
                        .Where(c => c.StartOn == previousStartOn.Value)
                        .ToListAsync()
                    : new List<Cash_vwFlowCategoryByPeriod>();
            }
            else
            {
                previousRows = new List<Cash_vwFlowCategoryByPeriod>();
            }

            return BuildProfitAndLossDetailSections(
                categories,
                currentRows.Select(r => (
                    r.CategoryCode,
                    r.CashCode,
                    cashCodes.TryGetValue(r.CashCode, out var description) ? description : r.CashCode,
                    r.InvoiceValue ?? 0m)),
                previousRows.Select(r => (
                    r.CategoryCode,
                    r.CashCode,
                    cashCodes.TryGetValue(r.CashCode, out var description) ? description : r.CashCode,
                    r.InvoiceValue ?? 0m)));
        }

        private static IReadOnlyList<TaxHubProfitAndLossDetailSection> BuildProfitAndLossDetailSections<TCategory>(
            IEnumerable<TCategory> categories,
            IEnumerable<(string CategoryCode, string CashCode, string CashDescription, decimal InvoiceValue)> currentRows,
            IEnumerable<(string CategoryCode, string CashCode, string CashDescription, decimal InvoiceValue)> previousRows)
            where TCategory : class
        {
            var previousLookup = previousRows.ToDictionary(
                r => $"{r.CategoryCode}|{r.CashCode}",
                r => r.InvoiceValue,
                StringComparer.OrdinalIgnoreCase);

            var currentGrouped = currentRows
                .GroupBy(r => r.CategoryCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            var result = new List<TaxHubProfitAndLossDetailSection>();

            foreach (var category in categories)
            {
                var categoryCode = (string)category!.GetType().GetProperty("CategoryCode")!.GetValue(category)!;
                var categoryName = (string)category.GetType().GetProperty("Category")!.GetValue(category)!;

                if (!currentGrouped.TryGetValue(categoryCode, out var detailRows) || detailRows.Count == 0)
                    continue;

                var rows = detailRows
                    .Select(row =>
                    {
                        previousLookup.TryGetValue($"{row.CategoryCode}|{row.CashCode}", out var previousValue);

                        return new TaxHubProfitAndLossDetailRow
                        {
                            CashCode = row.CashCode,
                            CashDescription = row.CashDescription,
                            CurrentValue = row.InvoiceValue,
                            PreviousValue = previousValue
                        };
                    })
                    .OrderBy(r => r.CashCode)
                    .ToList();

                result.Add(new TaxHubProfitAndLossDetailSection
                {
                    CategoryCode = categoryCode,
                    Category = categoryName,
                    Rows = rows,
                    CurrentTotal = rows.Sum(r => r.CurrentValue),
                    PreviousTotal = rows.Sum(r => r.PreviousValue)
                });
            }

            return result;
        }

        private static IReadOnlyList<TaxHubProfitAndLossRow> BuildProfitAndLossRows<T>(
            IEnumerable<T> current,
            IEnumerable<T> previous)
            where T : class
        {
            var previousMap = previous.ToDictionary(
                item => (string)item.GetType().GetProperty("CategoryCode")!.GetValue(item)!,
                item => Convert.ToDecimal(item.GetType().GetProperty("InvoiceValue")!.GetValue(item)!));

            return current
                .Select(item =>
                {
                    var categoryCode = (string)item.GetType().GetProperty("CategoryCode")!.GetValue(item)!;
                    var category = (string)item.GetType().GetProperty("Category")!.GetValue(item)!;
                    var currentValue = Convert.ToDecimal(item.GetType().GetProperty("InvoiceValue")!.GetValue(item)!);

                    previousMap.TryGetValue(categoryCode, out var previousValue);

                    return new TaxHubProfitAndLossRow
                    {
                        CategoryCode = categoryCode,
                        Category = category,
                        CurrentValue = currentValue,
                        PreviousValue = previousValue
                    };
                })
                .ToList();
        }

        private TaxHubDashboardCard BuildVatCard(
            Cash_vwTaxVatSubmission? vatTotals,
            IReadOnlyCollection<TaxHubObligationSummary> obligations,
            TaxHubProjectedTaxDue projectedDue)
        {
            var vatObligation = obligations.FirstOrDefault(o => o.TaxTypeCode == 1);

            return new TaxHubDashboardCard
            {
                Workspace = TaxHubWorkspace.Vat,
                Title = "VAT",
                Subtitle = vatTotals?.Period ?? "No VAT totals available",
                Status = vatTotals is null ? "Not available" : "Active",
                PrimaryValue = projectedDue.VatDue == 0m ? "—" : projectedDue.VatDue.ToString("C"),
                SecondaryValue = vatObligation?.NextPaymentDueOn?.ToString("dd MMM yyyy") ?? "No payment due date",
                Detail = vatObligation?.NextFilingDueOn is null
                    ? "No filing obligation available"
                    : $"Next filing due {vatObligation.NextFilingDueOn:dd MMM yyyy}"
            };
        }

        private async Task<TaxHubDashboardCard> BuildBusinessTaxCard(
            Cash_vwTaxBizTotal? businessTaxTotals,
            IReadOnlyCollection<TaxHubObligationSummary> obligations,
            TaxHubProjectedTaxDue projectedDue)
        {
            NodeSettings nodeSettings = new NodeSettings(_nodeContext);
            var bizTypeCode = await nodeSettings.BizTaxType();
            var businessTaxObligation = obligations.FirstOrDefault(o => o.TaxTypeCode == (short)bizTypeCode);

            return new TaxHubDashboardCard
            {
                Workspace = TaxHubWorkspace.BusinessTax,
                Title = "Business Tax",
                Subtitle = businessTaxTotals?.Period ?? "No Business Tax totals available",
                Status = businessTaxTotals is null ? "Not available" : "Active",
                PrimaryValue = projectedDue.BusinessTaxDue == 0m ? "—" : projectedDue.BusinessTaxDue.ToString("C"),
                SecondaryValue = businessTaxObligation?.NextPaymentDueOn?.ToString("dd MMM yyyy") ?? "No payment due date",
                Detail = businessTaxObligation?.NextFilingDueOn is null
                    ? "No filing obligation available"
                    : $"Next filing due {businessTaxObligation.NextFilingDueOn:dd MMM yyyy}"
            };
        }

        private static TaxHubDashboardCard BuildAccountsCard(TaxHubAccountsValidationSummary summary)
        {
            var status = summary.Status switch
            {
                "FAIL" => "Fail",
                "WARN" => "Warning",
                "PASS" => "Pass",
                _ => "Pending"
            };

            var primaryValue = summary.TotalRows == 0
                ? "—"
                : $"{summary.PassCount}/{summary.TotalRows}";

            var secondaryValue = summary.TotalRows == 0
                ? "No validation rows"
                : $"{summary.FailCount} fail, {summary.WarnCount} warning";

            var detail = summary.TotalRows == 0
                ? "Equity bridge validation has not returned any rows."
                : $"Tolerance {summary.Tolerance:#,##0.00;(#,##0.00);-}. PASS {summary.PassCount}, WARN {summary.WarnCount}, FAIL {summary.FailCount}.";

            return new TaxHubDashboardCard
            {
                Workspace = TaxHubWorkspace.Accounts,
                Title = "Accounts Validation",
                Subtitle = "Equity bridge reconciliation",
                Status = status,
                PrimaryValue = primaryValue,
                SecondaryValue = secondaryValue,
                Detail = detail
            };
        }

        private async Task<TaxHubDueDateWindow?> GetNextDueDateAsync(short taxTypeCode, bool isAccrual)
        {
            var dueDates = new List<TaxHubDueDateWindow>();

            var connection = _nodeContext.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;

            if (shouldClose)
                await connection.OpenAsync();

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT PayOn, PayFrom, PayTo
                    FROM Cash.fnTaxTypeDueDates(@TaxTypeCode, @IsAccrual)
                    ORDER BY PayOn;";

                var taxTypeParameter = command.CreateParameter();
                taxTypeParameter.ParameterName = "@TaxTypeCode";
                taxTypeParameter.DbType = DbType.Int16;
                taxTypeParameter.Value = taxTypeCode;
                command.Parameters.Add(taxTypeParameter);

                var accrualParameter = command.CreateParameter();
                accrualParameter.ParameterName = "@IsAccrual";
                accrualParameter.DbType = DbType.Boolean;
                accrualParameter.Value = isAccrual;
                command.Parameters.Add(accrualParameter);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    dueDates.Add(new TaxHubDueDateWindow
                    {
                        PayOn = reader.IsDBNull(0) ? DateTime.MinValue : reader.GetDateTime(0),
                        PayFrom = reader.IsDBNull(1) ? null : reader.GetDateTime(1),
                        PayTo = reader.IsDBNull(2) ? null : reader.GetDateTime(2)
                    });
                }
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }

            var today = DateTime.Today;

            return dueDates
                .Where(d => d.PayOn.Date >= today)
                .OrderBy(d => d.PayOn)
                .FirstOrDefault()
                ?? dueDates.OrderByDescending(d => d.PayOn).FirstOrDefault();
        }
    }
}
