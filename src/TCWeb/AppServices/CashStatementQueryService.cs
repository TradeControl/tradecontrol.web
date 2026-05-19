using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Pages.Cash.Manager.Components;

namespace TradeControl.Web.AppServices
{
    public sealed class CashStatementQueryService : ICashStatementQueryService
    {
        private readonly NodeContext _nodeContext;
        private readonly ICashNamespaceResolver _namespaceResolver;

        public CashStatementQueryService(NodeContext nodeContext, ICashNamespaceResolver namespaceResolver)
        {
            _nodeContext = nodeContext;
            _namespaceResolver = namespaceResolver;
        }

        public async Task<CashManagerStatementResult> GetStatementAsync(
            string accountCode,
            short yearNumber,
            DateTime? periodStartOn,
            string namespaceFilter,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(accountCode))
            {
                return CashManagerStatementResult.Empty;
            }

            var yearPeriods = await _nodeContext.App_Periods
                .AsNoTracking()
                .Where(period =>
                    period.YearNumber == yearNumber
                    && period.CashStatusCode != (short)NodeEnum.CashStatus.Archived)
                .OrderBy(period => period.StartOn)
                .Select(period => period.StartOn)
                .ToListAsync(cancellationToken);

            if (yearPeriods.Count == 0)
            {
                return CashManagerStatementResult.Empty;
            }

            var effectivePeriodStartOn = periodStartOn ?? yearPeriods[0];
            var visiblePeriods = periodStartOn.HasValue
                ? yearPeriods.Where(startOn => startOn == periodStartOn.Value).ToList()
                : yearPeriods;

            var openingBalance = await GetOpeningBalanceAsync(accountCode, effectivePeriodStartOn, cancellationToken);
            var postedRows = await BuildPostedRowsAsync(accountCode, visiblePeriods, cancellationToken);
            var unpostedRows = await BuildUnpostedRowsAsync(accountCode, cancellationToken);

            var rows = postedRows
                .Concat(unpostedRows)
                .OrderBy(row => row.PaidOn)
                .ThenBy(row => GetStatusSortOrder(row.Status))
                .ThenBy(row => row.RowNumber ?? long.MaxValue)
                .ThenBy(row => row.PaymentCode, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var runningBalance = openingBalance;
            var rowsWithBalance = rows
                .Select(row => {
                    runningBalance += row.DeltaValue;
                    return row with { RunningBalance = runningBalance };
                })
                .ToList();

            var filteredRows = ApplyNamespaceFilter(rowsWithBalance, namespaceFilter);
            var groups = BuildGroups(filteredRows);
            var summary = BuildSummary(openingBalance, filteredRows);

            return new CashManagerStatementResult(summary, filteredRows, groups);
        }

        private async Task<decimal> GetOpeningBalanceAsync(
            string accountCode,
            DateTime effectivePeriodStartOn,
            CancellationToken cancellationToken)
        {
            return await _nodeContext.Cash_AccountStatements
                .AsNoTracking()
                .Where(statement => statement.AccountCode == accountCode && statement.StartOn < effectivePeriodStartOn)
                .OrderByDescending(statement => statement.PaidOn)
                .ThenByDescending(statement => statement.EntryNumber)
                .Select(statement => (decimal?)statement.PaidBalance)
                .FirstOrDefaultAsync(cancellationToken) ?? 0m;
        }

        private async Task<IReadOnlyList<CashManagerStatementRow>> BuildPostedRowsAsync(
            string accountCode,
            IReadOnlyList<DateTime> visiblePeriods,
            CancellationToken cancellationToken)
        {
            var statementRows = await _nodeContext.Cash_AccountStatements
                .AsNoTracking()
                .Where(statement => statement.AccountCode == accountCode && visiblePeriods.Contains(statement.StartOn))
                .OrderBy(statement => statement.PaidOn)
                .ThenBy(statement => statement.EntryNumber)
                .ToListAsync(cancellationToken);

            var rows = new List<CashManagerStatementRow>(statementRows.Count);

            foreach (var statement in statementRows)
            {
                var subjectCode = statement.SubjectCode ?? string.Empty;
                var parentSubjectCode = statement.ParentSubjectCode;
                var namespacePath = await _namespaceResolver.ResolveNamespacePathAsync(
                    subjectCode,
                    parentSubjectCode,
                    cancellationToken);

                rows.Add(new CashManagerStatementRow(
                    statement.EntryNumber,
                    statement.PaymentCode ?? string.Empty,
                    statement.PaidOn,
                    subjectCode,
                    statement.SubjectName ?? subjectCode,
                    parentSubjectCode,
                    namespacePath,
                    statement.PaymentReference ?? string.Empty,
                    statement.CashCode ?? string.Empty,
                    statement.CashDescription ?? string.Empty,
                    statement.TaxCode ?? string.Empty,
                    statement.TaxDescription ?? string.Empty,
                    statement.UserName ?? string.Empty,
                    statement.PaidInValue,
                    statement.PaidOutValue,
                    statement.PaidInValue - statement.PaidOutValue,
                    0m,
                    CashManagerRowStatus.Posted,
                    false));
            }

            return rows;
        }

        private async Task<IReadOnlyList<CashManagerStatementRow>> BuildUnpostedRowsAsync(
            string accountCode,
            CancellationToken cancellationToken)
        {
            var paymentRows = await (
                from payment in _nodeContext.Cash_tbPayments.AsNoTracking()
                join paymentView in _nodeContext.Cash_Payments.AsNoTracking()
                    on payment.PaymentCode equals paymentView.PaymentCode
                where payment.AccountCode == accountCode
                   && payment.PaymentStatusCode != (short)NodeEnum.PaymentStatus.Posted
                orderby payment.PaidOn, payment.PaymentCode
                select new UnpostedPaymentRow(
                    payment.PaymentCode,
                    payment.PaidOn,
                    payment.SubjectCode,
                    payment.ParentSubjectCode,
                    paymentView.SubjectName,
                    payment.PaymentReference ?? paymentView.PaymentReference,
                    payment.CashCode ?? paymentView.CashCode,
                    paymentView.CashDescription,
                    payment.TaxCode ?? paymentView.TaxCode,
                    paymentView.TaxDescription,
                    paymentView.UserName,
                    payment.PaidInValue,
                    payment.PaidOutValue,
                    payment.PaymentStatusCode))
                .ToListAsync(cancellationToken);

            var rows = new List<CashManagerStatementRow>(paymentRows.Count);

            foreach (var payment in paymentRows)
            {
                var subjectCode = payment.SubjectCode ?? string.Empty;
                var namespacePath = await _namespaceResolver.ResolveNamespacePathAsync(
                    subjectCode,
                    payment.ParentSubjectCode,
                    cancellationToken);

                var status = payment.PaymentStatusCode == (short)NodeEnum.PaymentStatus.Transfer
                    ? CashManagerRowStatus.Transfer
                    : CashManagerRowStatus.Unposted;

                rows.Add(new CashManagerStatementRow(
                    null,
                    payment.PaymentCode,
                    payment.PaidOn,
                    subjectCode,
                    payment.SubjectName ?? subjectCode,
                    payment.ParentSubjectCode,
                    namespacePath,
                    payment.PaymentReference ?? string.Empty,
                    payment.CashCode ?? string.Empty,
                    payment.CashDescription ?? string.Empty,
                    payment.TaxCode ?? string.Empty,
                    payment.TaxDescription ?? string.Empty,
                    payment.UserName ?? string.Empty,
                    payment.PaidInValue,
                    payment.PaidOutValue,
                    payment.PaidInValue - payment.PaidOutValue,
                    0m,
                    status,
                    true));
            }

            return rows;
        }

        private static IReadOnlyList<CashManagerStatementRow> ApplyNamespaceFilter(
            IReadOnlyList<CashManagerStatementRow> rows,
            string namespaceFilter)
        {
            var normalizedFilter = NormalizeNamespaceFilter(namespaceFilter);

            if (string.IsNullOrWhiteSpace(normalizedFilter))
            {
                return rows;
            }

            return rows
                .Where(row => MatchesNamespaceFilter(row, normalizedFilter))
                .ToList();
        }

        private static string NormalizeNamespaceFilter(string namespaceFilter)
        {
            return (namespaceFilter ?? string.Empty)
                .Trim()
                .TrimEnd('%')
                .Trim();
        }

        private static bool MatchesNamespaceFilter(
            CashManagerStatementRow row,
            string normalizedFilter)
        {
            var namespacePath = row.NamespacePath ?? string.Empty;
            var parentSubjectCode = row.ParentSubjectCode ?? string.Empty;
            var subjectCode = row.SubjectCode ?? string.Empty;

            if (namespacePath.StartsWith(normalizedFilter, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var segments = normalizedFilter
                .Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (segments.Length >= 2)
            {
                var filterParentSubjectCode = segments[^2];
                var filterSubjectCode = segments[^1];

                if (subjectCode.Equals(filterSubjectCode, StringComparison.OrdinalIgnoreCase)
                    && parentSubjectCode.Equals(filterParentSubjectCode, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return parentSubjectCode.StartsWith(normalizedFilter, StringComparison.OrdinalIgnoreCase)
                || subjectCode.StartsWith(normalizedFilter, StringComparison.OrdinalIgnoreCase);
        }

        private static IReadOnlyList<CashManagerStatementGroup> BuildGroups(
            IReadOnlyList<CashManagerStatementRow> rows)
        {
            return rows
                .GroupBy(
                    row => row.ParentSubjectCode ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase)
                .Select(group => {
                    var groupRows = group.ToList();

                    var namespacePath = groupRows
                        .Select(row => row.NamespacePath)
                        .FirstOrDefault(path => !string.IsNullOrWhiteSpace(path))
                        ?? "Unassigned";

                    var postedNet = groupRows
                        .Where(row => row.Status == CashManagerRowStatus.Posted)
                        .Sum(row => row.DeltaValue);

                    var unpostedNet = groupRows
                        .Where(row => row.Status != CashManagerRowStatus.Posted)
                        .Sum(row => row.DeltaValue);

                    return new CashManagerStatementGroup(
                        string.IsNullOrWhiteSpace(group.Key) ? "Unassigned" : group.Key,
                        string.IsNullOrWhiteSpace(group.Key) ? "Unassigned namespace" : group.Key,
                        namespacePath,
                        postedNet,
                        unpostedNet,
                        postedNet + unpostedNet,
                        groupRows);
                })
                .OrderBy(group => group.NamespacePath, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static CashManagerStatementSummary BuildSummary(
            decimal openingBalance,
            IReadOnlyList<CashManagerStatementRow> rows)
        {
            var postedNet = rows
                .Where(row => row.Status == CashManagerRowStatus.Posted)
                .Sum(row => row.DeltaValue);

            var unpostedNet = rows
                .Where(row => row.Status != CashManagerRowStatus.Posted)
                .Sum(row => row.DeltaValue);

            return new CashManagerStatementSummary(
                openingBalance,
                postedNet,
                unpostedNet,
                openingBalance + postedNet + unpostedNet,
                rows.Count(row => row.Status == CashManagerRowStatus.Posted),
                rows.Count(row => row.Status != CashManagerRowStatus.Posted));
        }

        private static int GetStatusSortOrder(CashManagerRowStatus status)
        {
            return status == CashManagerRowStatus.Posted ? 1 : 0;
        }

        private sealed record UnpostedPaymentRow(
            string PaymentCode,
            DateTime PaidOn,
            string SubjectCode,
            string? ParentSubjectCode,
            string? SubjectName,
            string? PaymentReference,
            string? CashCode,
            string? CashDescription,
            string? TaxCode,
            string? TaxDescription,
            string? UserName,
            decimal PaidInValue,
            decimal PaidOutValue,
            short PaymentStatusCode);
    }
}
