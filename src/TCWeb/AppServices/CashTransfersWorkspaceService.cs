using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeControl.Web.Data;
using TradeControl.Web.Models;
using TradeControl.Web.Pages.Cash.Manager.Components;

namespace TradeControl.Web.AppServices
{
    public sealed class CashTransfersWorkspaceService : ICashTransfersWorkspaceService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public CashTransfersWorkspaceService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<CashManagerTransfersWorkspaceState> GetWorkspaceAsync(
            string sourceAccountCode,
            short yearNumber,
            DateTime? periodStartOn,
            string aspNetUserId,
            bool isPrivileged,
            CancellationToken cancellationToken = default)
        {
            sourceAccountCode = NormalizeCode(sourceAccountCode);

            if (string.IsNullOrWhiteSpace(sourceAccountCode))
            {
                return CashManagerTransfersWorkspaceState.Empty;
            }

            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var userId = await new Profile(nodeContext).UserId(aspNetUserId);
            var currentBalance = await nodeContext.Subject_CashAccounts
                .AsNoTracking()
                .Where(item => item.AccountCode == sourceAccountCode)
                .Select(item => (decimal?)item.CurrentBalance)
                .FirstOrDefaultAsync(cancellationToken) ?? 0m;

            var visiblePeriodSet = await GetVisiblePeriodSetAsync(
                nodeContext,
                yearNumber,
                periodStartOn,
                cancellationToken);

            var rowsQuery =
                from payment in nodeContext.Cash_Payments.AsNoTracking()
                join transferCode in nodeContext.Cash_TransferCodeLookup.AsNoTracking()
                    on payment.CashCode equals transferCode.CashCode
                where payment.AccountCode == sourceAccountCode
                    && payment.PaymentStatusCode == (short)NodeEnum.PaymentStatus.Transfer
                select payment;

            if (!isPrivileged)
            {
                rowsQuery = rowsQuery.Where(item => item.UserId == userId);
            }

            var rows = await rowsQuery
                .OrderByDescending(item => item.PaidOn)
                .ThenByDescending(item => item.PaymentCode)
                .ToListAsync(cancellationToken);

            var plannedTransfers = rows
                .Where(item => visiblePeriodSet.Count == 0 || visiblePeriodSet.Contains(new DateTime(item.PaidOn.Year, item.PaidOn.Month, 1)))
                .Select(item => new CashManagerTransferRow(
                    item.PaymentCode,
                    item.UserName ?? string.Empty,
                    item.PaidOn,
                    item.PaymentReference ?? string.Empty,
                    item.CashCode ?? string.Empty,
                    item.CashDescription ?? string.Empty,
                    item.PaidOutValue,
                    item.PaidInValue))
                .ToList();

            var transferCodes = await GetTransferCodesAsync(nodeContext, cancellationToken);
            var paidOutTransferCodes = transferCodes
                .Where(item => item.CashPolarityCode == 0)
                .ToList();
            var paidInTransferCodes = transferCodes
                .Where(item => item.CashPolarityCode == 1)
                .ToList();

            var draft = new CashManagerTransferDraftModel {
                PaidOn = DateTime.Today,
                PaidOutCashCode = paidOutTransferCodes.FirstOrDefault()?.CashCode ?? string.Empty,
                PaidInCashCode = paidInTransferCodes.FirstOrDefault()?.CashCode ?? string.Empty
            };

            var destinations = await nodeContext.Subject_tbAccounts
                .AsNoTracking()
                .Where(item =>
                    !item.AccountClosed
                    && item.AccountTypeCode == (short)NodeEnum.CashAccountType.Cash
                    && item.CoinTypeCode == (short)NodeEnum.CoinType.Fiat
                    && item.AccountCode != sourceAccountCode)
                .OrderBy(item => item.AccountName)
                .Select(item => new CashManagerTransferAccountOption(
                    item.AccountCode,
                    item.AccountName))
                .ToListAsync(cancellationToken);

            return new CashManagerTransfersWorkspaceState(
                new CashManagerTransferEntrySummary(
                    currentBalance,
                    plannedTransfers.Sum(item => item.PaidInValue - item.PaidOutValue),
                    plannedTransfers.Count),
                draft,
                destinations,
                paidOutTransferCodes,
                paidInTransferCodes,
                plannedTransfers);
        }

        public async Task AddAsync(
            string sourceAccountCode,
            CashManagerTransferDraftModel draft,
            string aspNetUserId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(draft);

            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            sourceAccountCode = NormalizeCode(sourceAccountCode);
            draft.DestinationAccountCode = NormalizeCode(draft.DestinationAccountCode);

            if (string.IsNullOrWhiteSpace(sourceAccountCode))
            {
                throw new InvalidOperationException("A source cash account is required.");
            }

            if (string.IsNullOrWhiteSpace(draft.DestinationAccountCode))
            {
                throw new InvalidOperationException("A destination cash account is required.");
            }

            if (string.Equals(sourceAccountCode, draft.DestinationAccountCode, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Source and destination cash accounts must be different.");
            }

            if (draft.Amount <= 0m)
            {
                throw new InvalidOperationException("A positive transfer amount is required.");
            }

            var transferCodes = await GetTransferCodesAsync(nodeContext, cancellationToken);
            var paidOutCashCode = ResolveTransferCashCode(
                draft.PaidOutCashCode,
                transferCodes,
                0,
                "A transfer pay out cash code is required.");
            var paidInCashCode = ResolveTransferCashCode(
                draft.PaidInCashCode,
                transferCodes,
                1,
                "A transfer pay in cash code is required.");

            var destinationExists = await nodeContext.Subject_tbAccounts
                .AsNoTracking()
                .AnyAsync(item =>
                    item.AccountCode == draft.DestinationAccountCode
                    && !item.AccountClosed
                    && item.AccountTypeCode == (short)NodeEnum.CashAccountType.Cash
                    && item.CoinTypeCode == (short)NodeEnum.CoinType.Fiat,
                    cancellationToken);

            if (!destinationExists)
            {
                throw new InvalidOperationException("The selected destination account is not valid for transfers.");
            }

            var paidOutTaxCode = await GetTaxCodeAsync(nodeContext, paidOutCashCode, cancellationToken);
            var paidInTaxCode = await GetTaxCodeAsync(nodeContext, paidInCashCode, cancellationToken);

            var profile = new Profile(nodeContext);
            var userId = await profile.UserId(aspNetUserId);
            var userName = await profile.UserName(aspNetUserId);
            var companySubjectCode = await profile.CompanySubjectCode();
            var cashAccounts = new CashAccounts(nodeContext);

            var paidOutPaymentCode = await cashAccounts.NextPaymentCode();

            if (string.IsNullOrWhiteSpace(paidOutPaymentCode))
            {
                throw new InvalidOperationException("The next transfer payment code could not be generated.");
            }

            nodeContext.Cash_PaymentsUnposted.Add(CreateTransferPayment(
                paidOutPaymentCode,
                userId,
                userName,
                companySubjectCode,
                sourceAccountCode,
                paidOutCashCode,
                paidOutTaxCode,
                draft.PaidOn,
                draft.Amount,
                0m,
                draft.PaymentReference));

            await nodeContext.SaveChangesAsync(cancellationToken);

            try
            {
                var paidInPaymentCode = await cashAccounts.NextPaymentCode();

                if (string.IsNullOrWhiteSpace(paidInPaymentCode))
                {
                    throw new InvalidOperationException("The matching transfer receipt payment code could not be generated.");
                }

                nodeContext.Cash_PaymentsUnposted.Add(CreateTransferPayment(
                    paidInPaymentCode,
                    userId,
                    userName,
                    companySubjectCode,
                    draft.DestinationAccountCode,
                    paidInCashCode,
                    paidInTaxCode,
                    draft.PaidOn,
                    0m,
                    draft.Amount,
                    draft.PaymentReference));

                await nodeContext.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                await cashAccounts.DeletePayment(paidOutPaymentCode);
                throw;
            }
        }

        public async Task<int> PostVisibleAsync(
            string sourceAccountCode,
            short yearNumber,
            DateTime? periodStartOn,
            string aspNetUserId,
            bool isPrivileged,
            CancellationToken cancellationToken = default)
        {
            sourceAccountCode = NormalizeCode(sourceAccountCode);

            if (string.IsNullOrWhiteSpace(sourceAccountCode))
            {
                return 0;
            }

            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var userId = await new Profile(nodeContext).UserId(aspNetUserId);
            var visiblePeriodSet = await GetVisiblePeriodSetAsync(
                nodeContext,
                yearNumber,
                periodStartOn,
                cancellationToken);

            var query = nodeContext.Cash_tbPayments
                .AsNoTracking()
                .Where(item =>
                    item.AccountCode == sourceAccountCode
                    && item.PaymentStatusCode == (short)NodeEnum.PaymentStatus.Transfer);

            if (!isPrivileged)
            {
                query = query.Where(item => item.UserId == userId);
            }

            var paymentCodes = await query
                .OrderBy(item => item.PaidOn)
                .ThenBy(item => item.PaymentCode)
                .Select(item => new { item.PaymentCode, item.PaidOn })
                .ToListAsync(cancellationToken);

            var visiblePaymentCodes = paymentCodes
                .Where(item => visiblePeriodSet.Count == 0 || visiblePeriodSet.Contains(new DateTime(item.PaidOn.Year, item.PaidOn.Month, 1)))
                .Select(item => item.PaymentCode)
                .ToList();

            if (visiblePaymentCodes.Count == 0)
            {
                return 0;
            }

            var cashAccounts = new CashAccounts(nodeContext);
            var postedCount = 0;

            foreach (var paymentCode in visiblePaymentCodes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!await cashAccounts.PostTransfer(paymentCode))
                {
                    throw new InvalidOperationException($"Transfer '{paymentCode}' could not be posted.");
                }

                postedCount++;
            }

            return postedCount;
        }

        private static Cash_vwPaymentsUnposted CreateTransferPayment(
            string paymentCode,
            string userId,
            string userName,
            string subjectCode,
            string accountCode,
            string cashCode,
            string taxCode,
            DateTime paidOn,
            decimal paidOutValue,
            decimal paidInValue,
            string paymentReference)
        {
            return new Cash_vwPaymentsUnposted {
                PaymentCode = paymentCode,
                UserId = userId,
                PaymentStatusCode = (short)NodeEnum.PaymentStatus.Transfer,
                SubjectCode = subjectCode,
                AccountCode = accountCode,
                CashCode = NormalizeNullableCode(cashCode),
                TaxCode = NormalizeNullableCode(taxCode),
                PaidOn = paidOn,
                PaidOutValue = paidOutValue,
                PaidInValue = paidInValue,
                PaymentReference = NormalizeNullableText(paymentReference),
                InsertedBy = userName,
                UpdatedBy = userName,
                InsertedOn = DateTime.Now,
                UpdatedOn = DateTime.Now
            };
        }

        private static async Task<HashSet<DateTime>> GetVisiblePeriodSetAsync(
            NodeContext nodeContext,
            short yearNumber,
            DateTime? periodStartOn,
            CancellationToken cancellationToken)
        {
            var periodsQuery = nodeContext.App_Periods
                .AsNoTracking()
                .Where(period => period.CashStatusCode != (short)NodeEnum.CashStatus.Archived);

            if (yearNumber > 0)
            {
                periodsQuery = periodsQuery.Where(period => period.YearNumber == yearNumber);
            }

            var yearPeriods = await periodsQuery
                .OrderBy(period => period.StartOn)
                .Select(period => period.StartOn)
                .ToListAsync(cancellationToken);

            if (yearPeriods.Count == 0)
            {
                return [];
            }

            if (periodStartOn.HasValue && yearPeriods.Contains(periodStartOn.Value))
            {
                return [periodStartOn.Value];
            }

            return yearPeriods.ToHashSet();
        }

        private static async Task<IReadOnlyList<CashManagerTransferCodeOption>> GetTransferCodesAsync(
            NodeContext nodeContext,
            CancellationToken cancellationToken)
        {
            return await nodeContext.Cash_TransferCodeLookup
                .AsNoTracking()
                .OrderBy(item => item.CashPolarityCode)
                .ThenBy(item => item.CashDescription)
                .Select(item => new CashManagerTransferCodeOption(
                    item.CashCode,
                    item.CashDescription,
                    item.CashPolarityCode))
                .ToListAsync(cancellationToken);
        }

        private static async Task<string> GetTaxCodeAsync(
            NodeContext nodeContext,
            string cashCode,
            CancellationToken cancellationToken)
        {
            return await nodeContext.Cash_tbCodes
                .AsNoTracking()
                .Where(item => item.CashCode == cashCode)
                .Select(item => item.TaxCode)
                .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;
        }

        private static string ResolveTransferCashCode(
            string requestedCashCode,
            IReadOnlyList<CashManagerTransferCodeOption> transferCodes,
            short requiredPolarityCode,
            string emptyMessage)
        {
            if (string.IsNullOrWhiteSpace(requestedCashCode))
            {
                var defaultCode = transferCodes
                    .FirstOrDefault(item => item.CashPolarityCode == requiredPolarityCode)
                    ?.CashCode;

                if (!string.IsNullOrWhiteSpace(defaultCode))
                {
                    return defaultCode;
                }

                throw new InvalidOperationException(emptyMessage);
            }

            var matchedCode = transferCodes.FirstOrDefault(item =>
                string.Equals(item.CashCode, requestedCashCode, StringComparison.OrdinalIgnoreCase));

            if (matchedCode is null)
            {
                throw new InvalidOperationException($"Transfer cash code '{requestedCashCode}' is not configured.");
            }

            if (matchedCode.CashPolarityCode != requiredPolarityCode)
            {
                var direction = requiredPolarityCode == 0 ? "pay out" : "pay in";
                throw new InvalidOperationException($"Transfer cash code '{requestedCashCode}' is not valid for the {direction} side.");
            }

            return matchedCode.CashCode;
        }

        private static string NormalizeCode(string? value)
        {
            return value?.Trim() ?? string.Empty;
        }

        private static string? NormalizeNullableCode(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
