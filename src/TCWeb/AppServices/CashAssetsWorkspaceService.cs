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
    public sealed class CashAssetsWorkspaceService : ICashAssetsWorkspaceService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public CashAssetsWorkspaceService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<CashManagerAssetsWorkspaceState> GetWorkspaceAsync(
            string accountCode,
            string aspNetUserId,
            bool isPrivileged,
            CancellationToken cancellationToken = default)
        {
            accountCode = NormalizeCode(accountCode);

            if (string.IsNullOrWhiteSpace(accountCode))
            {
                return CashManagerAssetsWorkspaceState.Empty;
            }

            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var userId = await GetUserIdAsync(nodeContext, aspNetUserId);
            var homeAccount = await nodeContext.App_HomeAccount
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            var currentBalance = await nodeContext.Subject_CashAccounts
                .AsNoTracking()
                .Where(item => item.AccountCode == accountCode)
                .Select(item => (decimal?)item.CurrentBalance)
                .FirstOrDefaultAsync(cancellationToken) ?? 0m;

            var rowsQuery = nodeContext.Cash_Payments
                .AsNoTracking()
                .Where(item =>
                    item.AccountCode == accountCode
                    && item.PaymentStatusCode != (short)NodeEnum.PaymentStatus.Posted);

            if (!isPrivileged)
            {
                rowsQuery = rowsQuery.Where(item => item.UserId == userId);
            }

            var rows = await rowsQuery
                .OrderByDescending(item => item.PaidOn)
                .ThenByDescending(item => item.PaymentCode)
                .Select(item => new CashManagerAssetRow(
                    item.PaymentCode,
                    item.UserName ?? string.Empty,
                    item.PaidOn,
                    item.SubjectCode,
                    item.SubjectName,
                    item.PaymentReference ?? string.Empty,
                    item.CashCode ?? string.Empty,
                    item.CashDescription ?? string.Empty,
                    item.PaidOutValue,
                    item.PaidInValue))
                .ToListAsync(cancellationToken);

            var summary = new CashManagerAssetEntrySummary(
                currentBalance,
                rows.Sum(item => item.PaidInValue - item.PaidOutValue),
                rows.Count);

            var draft = new CashManagerAssetDraftModel {
                NamespaceFilter = homeAccount?.SubjectCode ?? string.Empty,
                SubjectCode = homeAccount?.SubjectCode ?? string.Empty,
                SubjectName = homeAccount?.SubjectName ?? string.Empty,
                PaidOn = DateTime.Today,
                ReversalStartOn = DateTime.Today
            };

            return new CashManagerAssetsWorkspaceState(
                summary,
                draft,
                await GetCashCodesAsync(nodeContext, cancellationToken),
                rows);
        }

        public async Task<CashManagerAssetSubjectSelection> ResolveSubjectAsync(
            string namespaceFilter,
            CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var subject = await TryResolveSubjectAsync(nodeContext, namespaceFilter, cancellationToken);

            if (subject is null)
            {
                throw new InvalidOperationException("The selected subject was not found.");
            }

            return new CashManagerAssetSubjectSelection(
                subject.SubjectCode,
                subject.SubjectName,
                namespaceFilter.Trim());
        }

        public Task<IReadOnlyList<CashManagerPostedPaymentSearchResult>> SearchPostedPaymentsAsync(
            string assetAccountCode,
            string namespaceFilter,
            string searchText,
            string aspNetUserId,
            bool isPrivileged,
            int take = 40,
            CancellationToken cancellationToken = default)
        {
            return SearchPostedPaymentsByCashTypeAsync(
                assetAccountCode,
                namespaceFilter,
                searchText,
                aspNetUserId,
                isPrivileged,
                (short)NodeEnum.CashType.Trade,
                take,
                cancellationToken);
        }

        public async Task<IReadOnlyList<CashManagerPostedPaymentSearchResult>> SearchPostedCashPaymentsAsync(
            string assetAccountCode,
            string namespaceFilter,
            string searchText,
            string aspNetUserId,
            bool isPrivileged,
            int take = 40,
            CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            assetAccountCode = NormalizeCode(assetAccountCode);
            searchText = (searchText ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(assetAccountCode))
            {
                return Array.Empty<CashManagerPostedPaymentSearchResult>();
            }

            var userId = await GetUserIdAsync(nodeContext, aspNetUserId);
            var subject = await TryResolveSubjectAsync(nodeContext, namespaceFilter, cancellationToken);
            var subjectCode = subject?.SubjectCode ?? string.Empty;

            var query = nodeContext.Cash_Payments
                .AsNoTracking()
                .Where(item =>
                    item.PaymentStatusCode == (short)NodeEnum.PaymentStatus.Posted
                    && item.AccountCode == assetAccountCode);

            if (!isPrivileged)
            {
                query = query.Where(item => item.UserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(subjectCode))
            {
                query = query.Where(item => item.SubjectCode == subjectCode);
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(item =>
                    item.PaymentCode.Contains(searchText)
                    || (item.PaymentReference != null && item.PaymentReference.Contains(searchText))
                    || item.SubjectCode.Contains(searchText)
                    || item.SubjectName.Contains(searchText)
                    || item.AccountCode.Contains(searchText)
                    || item.AccountName.Contains(searchText)
                    || (item.CashCode != null && item.CashCode.Contains(searchText))
                    || (item.CashDescription != null && item.CashDescription.Contains(searchText)));
            }

            return await query
                .OrderByDescending(item => item.PaidOn)
                .ThenByDescending(item => item.PaymentCode)
                .Take(take)
                .Select(item => new CashManagerPostedPaymentSearchResult(
                    item.PaymentCode,
                    item.PaidOn,
                    item.AccountCode,
                    item.AccountName ?? string.Empty,
                    item.SubjectCode,
                    item.SubjectName,
                    item.PaymentReference ?? string.Empty,
                    item.CashCode ?? string.Empty,
                    item.CashDescription ?? string.Empty,
                    item.PaidOutValue,
                    item.PaidInValue,
                    item.PaidInValue - item.PaidOutValue))
                .ToListAsync(cancellationToken);
        }

        public async Task AddFreehandAsync(
            string accountCode,
            CashManagerAssetDraftModel draft,
            string aspNetUserId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(draft);

            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            accountCode = NormalizeCode(accountCode);

            if (string.IsNullOrWhiteSpace(accountCode))
            {
                throw new InvalidOperationException("An asset account is required.");
            }

            if ((draft.PaidInValue + draft.PaidOutValue) == 0m
                || (draft.PaidInValue != 0m && draft.PaidOutValue != 0m))
            {
                throw new InvalidOperationException("Enter either Paid In or Paid Out.");
            }

            if (string.IsNullOrWhiteSpace(draft.CashCode))
            {
                throw new InvalidOperationException("An asset cash code is required.");
            }

            var subject = await TryResolveSubjectAsync(nodeContext, draft.NamespaceFilter, cancellationToken);

            if (subject is null)
            {
                throw new InvalidOperationException("A subject or namespace selection is required.");
            }

            if (string.IsNullOrWhiteSpace(draft.TaxCode))
            {
                draft.TaxCode = await nodeContext.Cash_BankCashCodes
                    .AsNoTracking()
                    .Where(item => item.CashCode == draft.CashCode)
                    .Select(item => item.TaxCode)
                    .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;
            }

            var profile = new Profile(nodeContext);
            var userId = await profile.UserId(aspNetUserId);
            var userName = await profile.UserName(aspNetUserId);
            var paymentCode = await new CashAccounts(nodeContext).NextPaymentCode();

            var entity = new Cash_vwPaymentsUnposted {
                PaymentCode = paymentCode,
                UserId = userId,
                PaymentStatusCode = (short)NodeEnum.PaymentStatus.Unposted,
                SubjectCode = subject.SubjectCode,
                AccountCode = accountCode,
                CashCode = NormalizeNullableCode(draft.CashCode),
                TaxCode = NormalizeNullableCode(draft.TaxCode),
                PaidOn = draft.PaidOn,
                PaidInValue = draft.PaidInValue,
                PaidOutValue = draft.PaidOutValue,
                PaymentReference = NormalizeNullableText(draft.PaymentReference),
                InsertedBy = userName,
                UpdatedBy = userName,
                InsertedOn = DateTime.Now,
                UpdatedOn = DateTime.Now
            };

            nodeContext.Cash_PaymentsUnposted.Add(entity);
            await nodeContext.SaveChangesAsync(cancellationToken);
        }

        public async Task AddFromPaymentAsync(
            string accountCode,
            string sourcePaymentCode,
            CashManagerAssetDraftModel draft,
            string aspNetUserId,
            bool isPrivileged,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(draft);

            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            accountCode = NormalizeCode(accountCode);
            sourcePaymentCode = NormalizeCode(sourcePaymentCode);

            if (string.IsNullOrWhiteSpace(accountCode))
            {
                throw new InvalidOperationException("An asset account is required.");
            }

            if (string.IsNullOrWhiteSpace(sourcePaymentCode))
            {
                throw new InvalidOperationException("A source payment must be selected.");
            }

            var sourcePayment = await nodeContext.Cash_Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(item =>
                    item.PaymentCode == sourcePaymentCode
                    && item.PaymentStatusCode == (short)NodeEnum.PaymentStatus.Posted,
                    cancellationToken);

            if (sourcePayment is null)
            {
                throw new InvalidOperationException("The selected source payment was not found.");
            }

            if (!isPrivileged)
            {
                var userId = await GetUserIdAsync(nodeContext, aspNetUserId);

                if (!string.Equals(sourcePayment.UserId, userId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("You can only process your own posted payments into assets.");
                }
            }

            var deltaValue = sourcePayment.PaidInValue - sourcePayment.PaidOutValue;
            var absoluteValue = Math.Abs(deltaValue);

            if (absoluteValue == 0m)
            {
                throw new InvalidOperationException("The selected source payment has no value to process.");
            }

            draft.PaymentReference = string.IsNullOrWhiteSpace(draft.PaymentReference)
                ? sourcePayment.PaymentReference ?? sourcePayment.PaymentCode
                : draft.PaymentReference;

            var cashAccounts = new CashAccounts(nodeContext);

            if (draft.GenerateReversalSeries)
            {
                var reversalPeriods = (short)Math.Clamp(draft.ReversalPeriods, 1, short.MaxValue);
                var reversalIntervalMonths = (short)Math.Clamp(draft.ReversalIntervalMonths, 1, short.MaxValue);

                if (!await cashAccounts.AssetReversal(
                    sourcePayment.PaymentCode,
                    reversalPeriods,
                    reversalIntervalMonths,
                    draft.ReversalStartOn,
                    draft.PaymentReference))
                {
                    throw new InvalidOperationException($"Asset reversal failed for payment '{sourcePayment.PaymentCode}'.");
                }

                return;
            }

            if (string.IsNullOrWhiteSpace(draft.CashCode))
            {
                throw new InvalidOperationException("An asset cash code is required.");
            }

            var subject = await TryResolveSubjectAsync(nodeContext, draft.NamespaceFilter, cancellationToken);

            if (subject is null)
            {
                throw new InvalidOperationException("A subject or namespace selection is required.");
            }

            if (string.IsNullOrWhiteSpace(draft.TaxCode))
            {
                draft.TaxCode = await nodeContext.Cash_BankCashCodes
                    .AsNoTracking()
                    .Where(item => item.CashCode == draft.CashCode)
                    .Select(item => item.TaxCode)
                    .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;
            }

            var profile = new Profile(nodeContext);
            var userIdForInsert = await profile.UserId(aspNetUserId);
            var userName = await profile.UserName(aspNetUserId);
            var paymentCode = await cashAccounts.NextPaymentCode();

            var paidInValue = deltaValue < 0m ? absoluteValue : 0m;
            var paidOutValue = deltaValue < 0m ? 0m : absoluteValue;

            var entity = CreateUnpostedAssetEntity(
                paymentCode,
                userIdForInsert,
                userName,
                subject.SubjectCode,
                accountCode,
                draft.CashCode,
                draft.TaxCode,
                draft.PaidOn,
                paidInValue,
                paidOutValue,
                draft.PaymentReference);

            nodeContext.Cash_PaymentsUnposted.Add(entity);
            await nodeContext.SaveChangesAsync(cancellationToken);
        }

        private static Cash_vwPaymentsUnposted CreateUnpostedAssetEntity(
            string paymentCode,
            string userId,
            string userName,
            string subjectCode,
            string accountCode,
            string cashCode,
            string taxCode,
            DateTime paidOn,
            decimal paidInValue,
            decimal paidOutValue,
            string paymentReference)
        {
            return new Cash_vwPaymentsUnposted {
                PaymentCode = paymentCode,
                UserId = userId,
                PaymentStatusCode = (short)NodeEnum.PaymentStatus.Unposted,
                SubjectCode = subjectCode,
                AccountCode = accountCode,
                CashCode = NormalizeNullableCode(cashCode),
                TaxCode = NormalizeNullableCode(taxCode),
                PaidOn = paidOn,
                PaidInValue = paidInValue,
                PaidOutValue = paidOutValue,
                PaymentReference = NormalizeNullableText(paymentReference),
                InsertedBy = userName,
                UpdatedBy = userName,
                InsertedOn = DateTime.Now,
                UpdatedOn = DateTime.Now
            };
        }

        public async Task PostAsync(
            string paymentCode,
            string aspNetUserId,
            bool isPrivileged,
            CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            paymentCode = NormalizeCode(paymentCode);

            if (string.IsNullOrWhiteSpace(paymentCode))
            {
                return;
            }

            var payment = await nodeContext.Cash_tbPayments
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.PaymentCode == paymentCode, cancellationToken);

            if (payment is null)
            {
                return;
            }

            if (!isPrivileged)
            {
                var userId = await GetUserIdAsync(nodeContext, aspNetUserId);

                if (!string.Equals(payment.UserId, userId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("You can only post your own unposted asset entries.");
                }
            }

            var cashAccounts = new CashAccounts(nodeContext);

            if (!await cashAccounts.PostAsset(paymentCode))
            {
                throw new InvalidOperationException($"Asset post failed for payment '{paymentCode}'.");
            }

            await nodeContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(
            string paymentCode,
            string aspNetUserId,
            bool isPrivileged,
            CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            paymentCode = NormalizeCode(paymentCode);

            if (string.IsNullOrWhiteSpace(paymentCode))
            {
                return;
            }

            var payment = await nodeContext.Cash_tbPayments
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.PaymentCode == paymentCode, cancellationToken);

            if (payment is null)
            {
                return;
            }

            if (!isPrivileged)
            {
                var userId = await GetUserIdAsync(nodeContext, aspNetUserId);

                if (!string.Equals(payment.UserId, userId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("You can only delete your own unposted asset entries.");
                }
            }

            var cashAccounts = new CashAccounts(nodeContext);

            if (!await cashAccounts.DeletePayment(paymentCode))
            {
                throw new InvalidOperationException($"Asset entry '{paymentCode}' could not be deleted.");
            }
        }

        private static async Task<IReadOnlyList<CashManagerAssetCodeOption>> GetCashCodesAsync(
            NodeContext nodeContext,
            CancellationToken cancellationToken)
        {
            return await nodeContext.Cash_BankCashCodes
                .AsNoTracking()
                .OrderBy(item => item.CashDescription)
                .Select(item => new CashManagerAssetCodeOption(
                    item.CashCode,
                    item.CashDescription,
                    ((NodeEnum.CashPolarity)item.CashPolarityCode).ToString(),
                    string.Empty,
                    item.CashPolarityCode))
                .ToListAsync(cancellationToken);
        }

        private static async Task<Subject_tbSubject?> TryResolveSubjectAsync(
            NodeContext nodeContext,
            string namespaceFilter,
            CancellationToken cancellationToken)
        {
            var key = ExtractSubjectCode(namespaceFilter);

            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            var subject = await nodeContext.Subject_tbSubjects
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.SubjectCode == key, cancellationToken);

            if (subject is not null)
            {
                return subject;
            }

            return await nodeContext.Subject_tbSubjects
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.SubjectName == key, cancellationToken);
        }

        private static async Task<string> GetUserIdAsync(NodeContext nodeContext, string aspNetUserId)
        {
            return await new Profile(nodeContext).UserId(aspNetUserId);
        }

        private async Task<IReadOnlyList<CashManagerPostedPaymentSearchResult>> SearchPostedPaymentsByCashTypeAsync(
            string assetAccountCode,
            string namespaceFilter,
            string searchText,
            string aspNetUserId,
            bool isPrivileged,
            short cashTypeCode,
            int take,
            CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            assetAccountCode = NormalizeCode(assetAccountCode);
            searchText = (searchText ?? string.Empty).Trim();

            var userId = await GetUserIdAsync(nodeContext, aspNetUserId);
            var subject = await TryResolveSubjectAsync(nodeContext, namespaceFilter, cancellationToken);
            var subjectCode = subject?.SubjectCode ?? string.Empty;

            var query =
                from payment in nodeContext.Cash_Payments.AsNoTracking()
                join cashCode in nodeContext.Cash_CodeLookup.AsNoTracking()
                    on payment.CashCode equals cashCode.CashCode
                where payment.PaymentStatusCode == (short)NodeEnum.PaymentStatus.Posted
                    && payment.AccountCode != assetAccountCode
                    && cashCode.CashTypeCode == cashTypeCode
                select payment;

            if (!isPrivileged)
            {
                query = query.Where(item => item.UserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(subjectCode))
            {
                query = query.Where(item => item.SubjectCode == subjectCode);
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(item =>
                    item.PaymentCode.Contains(searchText)
                    || (item.PaymentReference != null && item.PaymentReference.Contains(searchText))
                    || item.SubjectCode.Contains(searchText)
                    || item.SubjectName.Contains(searchText)
                    || item.AccountCode.Contains(searchText)
                    || item.AccountName.Contains(searchText)
                    || (item.CashCode != null && item.CashCode.Contains(searchText))
                    || (item.CashDescription != null && item.CashDescription.Contains(searchText)));
            }

            return await query
                .OrderByDescending(item => item.PaidOn)
                .ThenByDescending(item => item.PaymentCode)
                .Take(take)
                .Select(item => new CashManagerPostedPaymentSearchResult(
                    item.PaymentCode,
                    item.PaidOn,
                    item.AccountCode,
                    item.AccountName ?? string.Empty,
                    item.SubjectCode,
                    item.SubjectName,
                    item.PaymentReference ?? string.Empty,
                    item.CashCode ?? string.Empty,
                    item.CashDescription ?? string.Empty,
                    item.PaidOutValue,
                    item.PaidInValue,
                    item.PaidInValue - item.PaidOutValue))
                .ToListAsync(cancellationToken);
        }

        private static string ExtractSubjectCode(string namespaceFilter)
        {
            var normalizedFilter = namespaceFilter?.Trim().Trim('.') ?? string.Empty;

            if (string.IsNullOrWhiteSpace(normalizedFilter))
            {
                return string.Empty;
            }

            var segments = normalizedFilter
                .Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return segments.Length == 0
                ? string.Empty
                : segments[^1];
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
