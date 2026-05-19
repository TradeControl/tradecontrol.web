using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;
using TradeControl.Web.Pages.Cash.Manager.Components;

namespace TradeControl.Web.AppServices
{
    public sealed class CashPaymentsWorkspaceService : ICashPaymentsWorkspaceService
    {
        private readonly NodeContext _nodeContext;

        public CashPaymentsWorkspaceService(NodeContext nodeContext)
        {
            _nodeContext = nodeContext;
        }

        public async Task<CashManagerPaymentsWorkspaceState> GetWorkspaceAsync(
            string accountCode,
            string aspNetUserId,
            bool isPrivileged,
            CancellationToken cancellationToken = default)
        {
            accountCode = NormalizeCode(accountCode);

            if (string.IsNullOrWhiteSpace(accountCode))
            {
                return CashManagerPaymentsWorkspaceState.Empty;
            }

            var userId = await GetUserIdAsync(aspNetUserId);
            var users = (await GetUsersAsync(cancellationToken)).ToList();

            if (users.Count == 0 && !string.IsNullOrWhiteSpace(userId))
            {
                users.Add(new CashManagerSelectOption(userId, userId));
            }

            var paymentsQuery = _nodeContext.Cash_Payments
                .AsNoTracking()
                .Where(item =>
                    item.AccountCode == accountCode
                    && item.PaymentStatusCode == (short)NodeEnum.PaymentStatus.Unposted);

            if (!isPrivileged)
            {
                paymentsQuery = paymentsQuery.Where(item => item.UserId == userId);
            }

            var payments = await paymentsQuery
                .OrderByDescending(item => item.PaidOn)
                .ThenByDescending(item => item.PaymentCode)
                .ToListAsync(cancellationToken);

            var balances = await BuildOutstandingBalancesAsync(payments, cancellationToken);
            var periodStatuses = await BuildPeriodStatusesAsync(payments, cancellationToken);

            var rows = payments
                .Select(item => CreateLine(item, balances, periodStatuses))
                .ToList();

            var summary = new CashManagerPaymentEntrySummary(
                rows.Sum(item => item.PaidInValue - item.PaidOutValue),
                rows.Count);

            var draft = new CashManagerPaymentLineModel {
                UserId = !string.IsNullOrWhiteSpace(userId)
                    ? userId
                    : users.FirstOrDefault()?.Value ?? string.Empty,
                PaidOn = DateTime.Today,
                PeriodStatus = await GetPeriodStatusAsync(DateTime.Today, cancellationToken)
            };

            return new CashManagerPaymentsWorkspaceState(
                summary,
                draft,
                rows,
                users,
                await GetCashCodesAsync(cancellationToken),
                await GetTaxCodesAsync(cancellationToken),
                await GetSubjectTypesAsync(cancellationToken));
        }

        public async Task<CashManagerOrganisationCreationResult> CreateOrganisationAsync(
            CashManagerOrganisationDraftModel model,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            var parentSubjectCode = ExtractSubjectCode(model.NamespaceFilter);

            if (string.IsNullOrWhiteSpace(parentSubjectCode))
            {
                throw new InvalidOperationException("A namespace must be selected before adding an organisation.");
            }

            if (!model.SubjectTypeCode.HasValue)
            {
                throw new InvalidOperationException("A Subject type is required.");
            }

            var subjects = new Subjects(_nodeContext);
            var result = await subjects.AddChildByTypeAsync(
                parentSubjectCode,
                model.SubjectName,
                model.SubjectTypeCode.Value);

            if (!result.Succeeded || string.IsNullOrWhiteSpace(result.SelectedSubjectCode))
            {
                throw new InvalidOperationException(result.Message);
            }

            return await ResolveOrganisationAsync(
                BuildCreatedPath(model.NamespaceFilter, result.SelectedSubjectCode),
                cancellationToken);
        }

        public async Task<CashManagerOrganisationCreationResult> ResolveOrganisationAsync(
            string namespaceFilter,
            CancellationToken cancellationToken = default)
        {
            var subject = await TryResolveSubjectAsync(namespaceFilter, cancellationToken);

            if (subject is null)
            {
                throw new InvalidOperationException("The selected organisation was not found.");
            }

            var subjects = new Subjects(_nodeContext, subject.SubjectCode);
            var outstandingBalance = await subjects.BalanceOutstandingAsync();
            var defaultTaxCode = await subjects.DefaultTaxCodeAsync();

            return new CashManagerOrganisationCreationResult(
                subject.SubjectCode,
                subject.SubjectName,
                namespaceFilter.Trim(),
                outstandingBalance,
                defaultTaxCode ?? string.Empty);
        }

        public async Task AddPaymentAsync(
            string accountCode,
            CashManagerPaymentLineModel draft,
            string aspNetUserId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(draft);

            var subjectCode = await ValidatePaymentAsync(
                accountCode,
                draft,
                aspNetUserId,
                isUpdate: false,
                isPrivileged: true,
                cancellationToken);

            var profile = new Profile(_nodeContext);
            var userId = string.IsNullOrWhiteSpace(draft.UserId)
                ? await profile.UserId(aspNetUserId)
                : draft.UserId.Trim();

            var userName = await profile.UserName(aspNetUserId);
            var paymentCode = await new CashAccounts(_nodeContext).NextPaymentCode();

            var entity = new Cash_vwPaymentsUnposted {
                PaymentCode = paymentCode,
                UserId = userId,
                PaymentStatusCode = (short)NodeEnum.PaymentStatus.Unposted,
                SubjectCode = subjectCode,
                AccountCode = accountCode.Trim(),
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

            _nodeContext.Cash_PaymentsUnposted.Add(entity);
            await _nodeContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdatePaymentAsync(
            CashManagerPaymentLineModel payment,
            string aspNetUserId,
            bool isPrivileged,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(payment);

            var entity = await _nodeContext.Cash_PaymentsUnposted
                .FirstOrDefaultAsync(item => item.PaymentCode == payment.PaymentCode, cancellationToken);

            if (entity is null)
            {
                throw new InvalidOperationException($"Payment '{payment.PaymentCode}' was not found.");
            }

            if (!isPrivileged)
            {
                var currentUserId = await GetUserIdAsync(aspNetUserId);

                if (!string.Equals(entity.UserId, currentUserId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("You can only edit your own unposted payments.");
                }
            }

            var subjectCode = await ValidatePaymentAsync(
                entity.AccountCode,
                payment,
                aspNetUserId,
                isUpdate: true,
                isPrivileged: isPrivileged,
                cancellationToken);

            var userName = await new Profile(_nodeContext).UserName(aspNetUserId);

            entity.UserId = NormalizeCode(payment.UserId);
            entity.SubjectCode = subjectCode;
            entity.CashCode = NormalizeNullableCode(payment.CashCode);
            entity.TaxCode = NormalizeNullableCode(payment.TaxCode);
            entity.PaidOn = payment.PaidOn;
            entity.PaidInValue = payment.PaidInValue;
            entity.PaidOutValue = payment.PaidOutValue;
            entity.PaymentReference = NormalizeNullableText(payment.PaymentReference);
            entity.UpdatedBy = userName;
            entity.UpdatedOn = DateTime.Now;

            await _nodeContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeletePaymentAsync(
            string paymentCode,
            string aspNetUserId,
            bool isPrivileged,
            CancellationToken cancellationToken = default)
        {
            paymentCode = NormalizeCode(paymentCode);

            if (string.IsNullOrWhiteSpace(paymentCode))
            {
                return;
            }

            var payment = await _nodeContext.Cash_tbPayments
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.PaymentCode == paymentCode, cancellationToken);

            if (payment is null)
            {
                return;
            }

            if (!isPrivileged)
            {
                var userId = await GetUserIdAsync(aspNetUserId);

                if (!string.Equals(payment.UserId, userId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("You can only delete your own unposted payments.");
                }
            }

            var cashAccounts = new CashAccounts(_nodeContext);

            if (!await cashAccounts.DeletePayment(paymentCode))
            {
                throw new InvalidOperationException($"Payment '{paymentCode}' could not be deleted.");
            }
        }

        public async Task PostAsync(
            string aspNetUserId,
            CancellationToken cancellationToken = default)
        {
            var userId = await GetUserIdAsync(aspNetUserId);
            var cashAccounts = new CashAccounts(_nodeContext);

            if (!await cashAccounts.PostPayment(userId))
            {
                throw new InvalidOperationException($"Payment post failed for user {userId}.");
            }
        }

        public async Task<NodeEnum.CashStatus> GetPeriodStatusAsync(
            DateTime paidOn,
            CancellationToken cancellationToken = default)
        {
            var startOn = new DateTime(paidOn.Year, paidOn.Month, 1);

            var statusCode = await _nodeContext.App_tbYearPeriods
                .AsNoTracking()
                .Where(item => item.StartOn == startOn)
                .Select(item => (short?)item.CashStatusCode)
                .FirstOrDefaultAsync(cancellationToken);

            return statusCode.HasValue
                ? (NodeEnum.CashStatus)statusCode.Value
                : NodeEnum.CashStatus.Current;
        }

        public async Task<string> GetDefaultTaxCodeForCashCodeAsync(
            string cashCode,
            CancellationToken cancellationToken = default)
        {
            cashCode = NormalizeCode(cashCode);

            if (string.IsNullOrWhiteSpace(cashCode))
            {
                return string.Empty;
            }

            return await _nodeContext.Cash_CodeLookup
                .AsNoTracking()
                .Where(item => item.CashCode == cashCode)
                .Select(item => item.TaxCode)
                .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;
        }

        private async Task<string> ValidatePaymentAsync(
            string accountCode,
            CashManagerPaymentLineModel payment,
            string aspNetUserId,
            bool isUpdate,
            bool isPrivileged,
            CancellationToken cancellationToken)
        {
            accountCode = NormalizeCode(accountCode);

            if (string.IsNullOrWhiteSpace(accountCode))
            {
                throw new InvalidOperationException("A cash account is required.");
            }

            payment.PeriodStatus = await GetPeriodStatusAsync(payment.PaidOn, cancellationToken);

            if (payment.PeriodStatus is NodeEnum.CashStatus.Closed or NodeEnum.CashStatus.Archived)
            {
                throw new InvalidOperationException("The selected Paid On date falls in a closed period.");
            }

            if ((payment.PaidInValue + payment.PaidOutValue) == 0m
                || (payment.PaidInValue != 0m && payment.PaidOutValue != 0m))
            {
                throw new InvalidOperationException("Enter either Paid In or Paid Out.");
            }

            if (string.IsNullOrWhiteSpace(payment.CashCode))
            {
                payment.TaxCode = string.Empty;
            }
            else if (string.IsNullOrWhiteSpace(payment.TaxCode)
                && payment.OutstandingBalance == 0m)
            {
                payment.TaxCode = await GetDefaultTaxCodeForCashCodeAsync(payment.CashCode, cancellationToken);
            }

            var subjectCode = await ResolveSubjectCodeAsync(
                payment.SubjectCode,
                payment.NamespaceFilter,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(subjectCode))
            {
                throw new InvalidOperationException(isUpdate
                    ? "The selected organisation could not be resolved."
                    : "Select or create an organisation before adding the payment.");
            }

            if (!isPrivileged && isUpdate)
            {
                var currentUserId = await GetUserIdAsync(aspNetUserId);

                if (!string.Equals(payment.UserId, currentUserId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("You can only save your own unposted payments.");
                }
            }

            return subjectCode;
        }

        private async Task<Dictionary<string, decimal>> BuildOutstandingBalancesAsync(
            IReadOnlyList<Cash_vwPayment> payments,
            CancellationToken cancellationToken)
        {
            var balances = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            foreach (var subjectCode in payments
                         .Select(item => item.SubjectCode)
                         .Where(subjectCode => !string.IsNullOrWhiteSpace(subjectCode))
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                cancellationToken.ThrowIfCancellationRequested();
                balances[subjectCode] = await new Subjects(_nodeContext, subjectCode).BalanceOutstandingAsync();
            }

            return balances;
        }

        private async Task<Dictionary<DateTime, NodeEnum.CashStatus>> BuildPeriodStatusesAsync(
            IReadOnlyList<Cash_vwPayment> payments,
            CancellationToken cancellationToken)
        {
            var startOnValues = payments
                .Select(item => new DateTime(item.PaidOn.Year, item.PaidOn.Month, 1))
                .Distinct()
                .ToList();

            return await _nodeContext.App_tbYearPeriods
                .AsNoTracking()
                .Where(item => startOnValues.Contains(item.StartOn))
                .ToDictionaryAsync(
                    item => item.StartOn,
                    item => (NodeEnum.CashStatus)item.CashStatusCode,
                    cancellationToken);
        }

        private static CashManagerPaymentLineModel CreateLine(
            Cash_vwPayment item,
            IReadOnlyDictionary<string, decimal> balances,
            IReadOnlyDictionary<DateTime, NodeEnum.CashStatus> periodStatuses)
        {
            var startOn = new DateTime(item.PaidOn.Year, item.PaidOn.Month, 1);

            return new CashManagerPaymentLineModel {
                IsExisting = true,
                PaymentCode = item.PaymentCode,
                UserId = item.UserId,
                PaidOn = item.PaidOn,
                SubjectCode = item.SubjectCode,
                SubjectName = item.SubjectName,
                NamespaceFilter = item.SubjectCode,
                PaymentReference = item.PaymentReference ?? string.Empty,
                PaidOutValue = item.PaidOutValue,
                PaidInValue = item.PaidInValue,
                CashCode = item.CashCode ?? string.Empty,
                TaxCode = item.TaxCode ?? string.Empty,
                OutstandingBalance = balances.TryGetValue(item.SubjectCode, out var balance) ? balance : 0m,
                PeriodStatus = periodStatuses.TryGetValue(startOn, out var status) ? status : NodeEnum.CashStatus.Current
            };
        }

        private async Task<string> ResolveSubjectCodeAsync(
            string subjectCode,
            string namespaceFilter,
            CancellationToken cancellationToken)
        {
            subjectCode = NormalizeCode(subjectCode);

            if (!string.IsNullOrWhiteSpace(subjectCode))
            {
                return subjectCode;
            }

            var subject = await TryResolveSubjectAsync(namespaceFilter, cancellationToken);
            return subject?.SubjectCode ?? string.Empty;
        }

        private async Task<Subject_tbSubject?> TryResolveSubjectAsync(
            string namespaceFilter,
            CancellationToken cancellationToken)
        {
            var key = ExtractSubjectCode(namespaceFilter);

            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            var subject = await _nodeContext.Subject_tbSubjects
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.SubjectCode == key, cancellationToken);

            if (subject is not null)
            {
                return subject;
            }

            return await _nodeContext.Subject_tbSubjects
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.SubjectName == key, cancellationToken);
        }

        private async Task<string> GetUserIdAsync(string aspNetUserId)
        {
            return await new Profile(_nodeContext).UserId(aspNetUserId);
        }

        private async Task<IReadOnlyList<CashManagerSelectOption>> GetUsersAsync(
            CancellationToken cancellationToken)
        {
            return await _nodeContext.Usr_tbUsers
                .AsNoTracking()
                .Where(item => item.IsEnabled != 0)
                .OrderBy(item => item.UserName)
                .Select(item => new CashManagerSelectOption(
                    item.UserId,
                    item.UserName))
                .ToListAsync(cancellationToken);
        }

        private async Task<IReadOnlyList<CashManagerSelectOption>> GetCashCodesAsync(
            CancellationToken cancellationToken)
        {
            var options = await _nodeContext.Cash_CodeLookup
                .AsNoTracking()
                .Where(item => item.CashTypeCode < (short)NodeEnum.CashType.Money)
                .OrderBy(item => item.CashDescription)
                .Select(item => new CashManagerSelectOption(
                    item.CashCode,
                    item.CashDescription))
                .ToListAsync(cancellationToken);

            options.Insert(0, new CashManagerSelectOption(string.Empty, string.Empty));
            return options;
        }

        private async Task<IReadOnlyList<CashManagerSelectOption>> GetTaxCodesAsync(
            CancellationToken cancellationToken)
        {
            var options = await _nodeContext.App_TaxCodes
                .AsNoTracking()
                .OrderBy(item => item.TaxDescription)
                .Select(item => new CashManagerSelectOption(
                    item.TaxCode,
                    item.TaxDescription))
                .ToListAsync(cancellationToken);

            options.Insert(0, new CashManagerSelectOption(string.Empty, string.Empty));
            return options;
        }

        private async Task<IReadOnlyList<CashManagerSelectOption>> GetSubjectTypesAsync(
            CancellationToken cancellationToken)
        {
            return await _nodeContext.Subject_tbTypes
                .AsNoTracking()
                .Where(item => item.SubjectClassCode != (short)NodeEnum.SubjectClass.Structural)
                .OrderBy(item => item.SubjectType)
                .Select(item => new CashManagerSelectOption(
                    item.SubjectTypeCode.ToString(),
                    item.SubjectType))
                .ToListAsync(cancellationToken);
        }

        private static string BuildCreatedPath(string namespaceFilter, string subjectCode)
        {
            var normalizedFilter = namespaceFilter?.Trim().Trim('.') ?? string.Empty;

            return string.IsNullOrWhiteSpace(normalizedFilter)
                ? subjectCode
                : $"{normalizedFilter}.{subjectCode}";
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
