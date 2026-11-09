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
using TradeControl.Web.Tenancy;

namespace TradeControl.Web.AppServices
{
    public sealed class CashAssetsWorkspaceService : ICashAssetsWorkspaceService
    {
        private const string CompaniesHouseAccountsTaxSourceCode = "UK-CO-ACCTS-2026";
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
            var today = scope.ServiceProvider.GetRequiredService<IBusinessClock>().TodayDateTime;

            var userId = await GetUserIdAsync(nodeContext, aspNetUserId);
            var homeAccount = await nodeContext.App_HomeAccount
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            var currentBalance = await nodeContext.Subject_CashAccounts
                .AsNoTracking()
                .Where(item => item.AccountCode == accountCode)
                .Select(item => (decimal?)item.CurrentBalance)
                .FirstOrDefaultAsync(cancellationToken) ?? 0m;

            var assetAccount = await nodeContext.Subject_CashAccountAssets
                .AsNoTracking()
                .Where(item => item.AccountCode == accountCode)
                .Select(item => new {
                    item.CashCode,
                    item.TaxCode
                })
                .FirstOrDefaultAsync(cancellationToken);

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
                PaidOn = today,
                ReversalStartOn = today,
                CashCode = assetAccount?.CashCode ?? string.Empty,
                TaxCode = assetAccount?.TaxCode ?? string.Empty
            };

            return new CashManagerAssetsWorkspaceState(
                summary,
                draft,
                await GetCashCodesAsync(nodeContext, assetAccount?.CashCode, cancellationToken),
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

            var configuredCashCode = await nodeContext.Subject_CashAccountAssets
                .AsNoTracking()
                .Where(item => item.AccountCode == assetAccountCode)
                .Select(item => item.CashCode)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(configuredCashCode))
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
                    && item.AccountCode == assetAccountCode
                    && item.CashCode == configuredCashCode);

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

            return await BuildPaymentSearchResultsAsync(nodeContext, query, take, cancellationToken);
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

            draft.TaxCode = await EnsureCompatibleAssetCashCodeAsync(
                nodeContext,
                accountCode,
                draft.CashCode,
                cancellationToken);

            var subject = await TryResolveSubjectAsync(nodeContext, draft.NamespaceFilter, cancellationToken);

            if (subject is null)
            {
                throw new InvalidOperationException("A subject or namespace selection is required.");
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
                await EnsureCompatibleAssetCashCodeAsync(
                    nodeContext,
                    accountCode,
                    sourcePayment.CashCode,
                    cancellationToken);

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

            draft.TaxCode = await EnsureCompatibleAssetCashCodeAsync(
                nodeContext,
                accountCode,
                draft.CashCode,
                cancellationToken);

            var subject = await TryResolveSubjectAsync(nodeContext, draft.NamespaceFilter, cancellationToken);

            if (subject is null)
            {
                throw new InvalidOperationException("A subject or namespace selection is required.");
            }

            var profile = new Profile(nodeContext);
            var userIdForInsert = await profile.UserId(aspNetUserId);
            var userName = await profile.UserName(aspNetUserId);
            var paymentCode = await cashAccounts.NextPaymentCode();
            var capitalValue = await GetCapitalValueAsync(
                nodeContext,
                sourcePayment.TaxCode,
                absoluteValue,
                cancellationToken);

            var paidInValue = deltaValue < 0m ? capitalValue : 0m;
            var paidOutValue = deltaValue < 0m ? 0m : capitalValue;

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

            await EnsureCompatibleAssetCashCodeAsync(
                nodeContext,
                payment.AccountCode,
                payment.CashCode,
                cancellationToken);

            if (!isPrivileged)
            {
                var userId = await GetUserIdAsync(nodeContext, aspNetUserId);

                if (!string.Equals(payment.UserId, userId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("You can only post your own unposted asset entries.");
                }
            }

            var cashAccounts = new CashAccounts(nodeContext);
            var touchesClosedPeriod = await IsClosedPeriodAsync(
                nodeContext,
                payment.PaidOn,
                cancellationToken);

            if (!await cashAccounts.PostAsset(paymentCode))
            {
                throw new InvalidOperationException($"Asset post failed for payment '{paymentCode}'.");
            }

            await nodeContext.SaveChangesAsync(cancellationToken);

            if (touchesClosedPeriod && !await cashAccounts.RebuildSystem())
            {
                throw new InvalidOperationException(
                    "The closed-period asset transaction was posted, but the financial system rebuild failed. Check Event Viewer before continuing.");
            }
        }

        public async Task PostScheduledWriteOffAsync(
            string paymentCode,
            DateOnly accountingDate,
            string aspNetUserId,
            bool isPrivileged,
            CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            paymentCode = NormalizeCode(paymentCode);
            if (string.IsNullOrWhiteSpace(paymentCode))
                throw new InvalidOperationException("A scheduled write-off payment code is required.");

            var payment = await nodeContext.Cash_tbPayments
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.PaymentCode == paymentCode, cancellationToken);

            if (payment is null
                || payment.PaymentStatusCode != (short)NodeEnum.PaymentStatus.Unposted)
            {
                throw new InvalidOperationException("The scheduled write-off is no longer available to post.");
            }

            if (payment.PaidInValue != 0m
                || payment.PaidOutValue <= 0m)
            {
                throw new InvalidOperationException("The selected transaction is not a recognised scheduled asset write-off.");
            }

            if (DateOnly.FromDateTime(payment.PaidOn) > accountingDate)
                throw new InvalidOperationException("Future scheduled write-offs cannot be posted from this year-end review.");

            var assetAccount = await nodeContext.Subject_tbAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(account => account.AccountCode == payment.AccountCode, cancellationToken);

            if (assetAccount is null
                || assetAccount.AccountTypeCode != (short)NodeEnum.CashAccountType.Asset)
                throw new InvalidOperationException("The scheduled transaction does not belong to an asset account.");

            await EnsureCompatibleAssetCashCodeAsync(
                nodeContext,
                payment.AccountCode,
                payment.CashCode,
                cancellationToken);

            var connection = nodeContext.Database.GetDbConnection();
            var shouldClose = connection.State != System.Data.ConnectionState.Open;
            if (shouldClose)
                await connection.OpenAsync(cancellationToken);

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT COUNT_BIG(1)
                    FROM Cash.vwTaxTagCashCode AS mapping
                    WHERE mapping.TaxSourceCode = @TaxSourceCode
                      AND mapping.CashCode = @CashCode
                      AND mapping.MappingRoot IN (N'CA-ASSET', N'CA-DEPREC');";

                var taxSource = command.CreateParameter();
                taxSource.ParameterName = "@TaxSourceCode";
                taxSource.Value = CompaniesHouseAccountsTaxSourceCode;
                command.Parameters.Add(taxSource);

                var cashCode = command.CreateParameter();
                cashCode.ParameterName = "@CashCode";
                cashCode.Value = (object?)assetAccount.CashCode ?? DBNull.Value;
                command.Parameters.Add(cashCode);

                var mappedCount = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
                if (mappedCount == 0)
                    throw new InvalidOperationException("The scheduled transaction is outside the mapped statutory fixed-asset population.");
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }

            if (!isPrivileged)
            {
                var userId = await GetUserIdAsync(nodeContext, aspNetUserId);
                if (!string.Equals(payment.UserId, userId, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("You can only post your own scheduled asset write-offs.");
            }

            var cashAccounts = new CashAccounts(nodeContext);
            var touchesClosedPeriod = await IsClosedPeriodAsync(
                nodeContext,
                payment.PaidOn,
                cancellationToken);

            if (!await cashAccounts.PostAsset(paymentCode))
                throw new InvalidOperationException($"Scheduled write-off '{paymentCode}' could not be posted.");

            await nodeContext.SaveChangesAsync(cancellationToken);

            if (touchesClosedPeriod && !await cashAccounts.RebuildSystem())
            {
                throw new InvalidOperationException(
                    "The closed-period write-off was posted, but the financial system rebuild failed. Check Event Viewer before continuing.");
            }
        }

        private static async Task<bool> IsClosedPeriodAsync(
            NodeContext nodeContext,
            DateTime paidOn,
            CancellationToken cancellationToken)
        {
            var periodStart = new DateTime(paidOn.Year, paidOn.Month, 1);

            var statusCode = await nodeContext.App_tbYearPeriods
                .AsNoTracking()
                .Where(period => period.StartOn == periodStart)
                .Select(period => (short?)period.CashStatusCode)
                .FirstOrDefaultAsync(cancellationToken);

            return statusCode.HasValue
                && ((NodeEnum.CashStatus)statusCode.Value == NodeEnum.CashStatus.Closed
                    || (NodeEnum.CashStatus)statusCode.Value == NodeEnum.CashStatus.Archived);
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

        public async Task UpdateAsync(
            string paymentCode,
            string accountCode,
            CashManagerAssetDraftModel draft,
            string aspNetUserId,
            bool isPrivileged,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(draft);

            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            paymentCode = NormalizeCode(paymentCode);
            accountCode = NormalizeCode(accountCode);

            if (string.IsNullOrWhiteSpace(paymentCode))
            {
                throw new InvalidOperationException("A payment code is required.");
            }

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

            draft.TaxCode = await EnsureCompatibleAssetCashCodeAsync(
                nodeContext,
                accountCode,
                draft.CashCode,
                cancellationToken);

            var payment = await nodeContext.Cash_tbPayments
                .FirstOrDefaultAsync(item => item.PaymentCode == paymentCode, cancellationToken);

            if (payment is null)
            {
                throw new InvalidOperationException("The selected asset transaction was not found.");
            }

            if (payment.PaymentStatusCode == (short)NodeEnum.PaymentStatus.Posted)
            {
                throw new InvalidOperationException("Posted asset transactions cannot be edited here.");
            }

            if (!string.Equals(payment.AccountCode, accountCode, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The selected asset transaction does not belong to this account.");
            }

            if (!isPrivileged)
            {
                var userId = await GetUserIdAsync(nodeContext, aspNetUserId);

                if (!string.Equals(payment.UserId, userId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("You can only edit your own unposted asset entries.");
                }
            }

            var subject = await TryResolveSubjectAsync(nodeContext, draft.NamespaceFilter, cancellationToken);

            if (subject is null)
            {
                throw new InvalidOperationException("A subject or namespace selection is required.");
            }

            var profile = new Profile(nodeContext);

            payment.SubjectCode = subject.SubjectCode;
            payment.AccountCode = accountCode;
            payment.CashCode = NormalizeNullableCode(draft.CashCode);
            payment.TaxCode = NormalizeNullableCode(draft.TaxCode);
            payment.PaidOn = draft.PaidOn;
            payment.PaidInValue = draft.PaidInValue;
            payment.PaidOutValue = draft.PaidOutValue;
            payment.PaymentReference = NormalizeNullableText(draft.PaymentReference);
            payment.UpdatedBy = await profile.UserName(aspNetUserId);
            payment.UpdatedOn = DateTime.Now;

            await nodeContext.SaveChangesAsync(cancellationToken);
        }

        private static async Task<IReadOnlyList<CashManagerAssetCodeOption>> GetCashCodesAsync(
            NodeContext nodeContext,
            string? configuredCashCode,
            CancellationToken cancellationToken)
        {
            configuredCashCode = NormalizeCode(configuredCashCode);

            if (string.IsNullOrWhiteSpace(configuredCashCode))
            {
                return Array.Empty<CashManagerAssetCodeOption>();
            }

            return await nodeContext.Cash_Codes
                .AsNoTracking()
                .Where(item =>
                    item.CashCode == configuredCashCode
                    && item.IsCashEnabled
                    && item.IsCategoryEnabled)
                .OrderBy(item => item.CashDescription)
                .Select(item => new CashManagerAssetCodeOption(
                    item.CashCode,
                    item.CashDescription,
                    item.CashPolarity,
                    item.CashType,
                    item.CashPolarityCode))
                .ToListAsync(cancellationToken);
        }

        private static async Task<string> EnsureCompatibleAssetCashCodeAsync(
            NodeContext nodeContext,
            string accountCode,
            string? cashCode,
            CancellationToken cancellationToken)
        {
            accountCode = NormalizeCode(accountCode);
            cashCode = NormalizeCode(cashCode);

            var assetAccount = await nodeContext.Subject_CashAccountAssets
                .AsNoTracking()
                .Where(item => item.AccountCode == accountCode)
                .Select(item => new {
                    item.CashCode,
                    item.TaxCode
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (assetAccount is null)
            {
                throw new InvalidOperationException("The selected account is not configured as an asset account.");
            }

            if (!string.Equals(assetAccount.CashCode, cashCode, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Asset transactions for this account must use Cash Code '{assetAccount.CashCode}'.");
            }

            return assetAccount.TaxCode ?? string.Empty;
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

            return await BuildPaymentSearchResultsAsync(nodeContext, query, take, cancellationToken);
        }

        private static async Task<IReadOnlyList<CashManagerPostedPaymentSearchResult>> BuildPaymentSearchResultsAsync(
            NodeContext nodeContext,
            IQueryable<Cash_vwPayment> query,
            int take,
            CancellationToken cancellationToken)
        {
            var rows = await query
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
                    item.PaidInValue - item.PaidOutValue,
                    item.TaxCode ?? string.Empty,
                    0m,
                    2))
                .ToListAsync(cancellationToken);

            var selectedTaxCodes = rows
                .Select(item => item.TaxCode)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (selectedTaxCodes.Length == 0)
            {
                return rows;
            }

            var taxRates = await nodeContext.App_tbTaxCodes
                .AsNoTracking()
                .Where(item => selectedTaxCodes.Contains(item.TaxCode))
                .ToDictionaryAsync(
                    item => item.TaxCode,
                    item => (item.TaxRate, item.Decimals),
                    StringComparer.OrdinalIgnoreCase,
                    cancellationToken);

            return rows
                .Select(item => taxRates.TryGetValue(item.TaxCode, out var tax)
                    ? item with { TaxRate = tax.TaxRate, TaxDecimals = tax.Decimals }
                    : item)
                .ToArray();
        }

        private static async Task<decimal> GetCapitalValueAsync(
            NodeContext nodeContext,
            string? taxCode,
            decimal sourceValue,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(taxCode))
            {
                return sourceValue;
            }

            var tax = await nodeContext.App_tbTaxCodes
                .AsNoTracking()
                .Where(item => item.TaxCode == taxCode)
                .Select(item => new { item.TaxRate, item.Decimals })
                .FirstOrDefaultAsync(cancellationToken);

            return tax is null
                ? sourceValue
                : CashManagerPostedPaymentSearchResult.CalculateCapitalValue(
                    sourceValue,
                    tax.TaxRate,
                    tax.Decimals);
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
