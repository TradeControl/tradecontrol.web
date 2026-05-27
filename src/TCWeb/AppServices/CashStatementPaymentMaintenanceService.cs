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
    public sealed class CashStatementPaymentMaintenanceService : ICashStatementPaymentMaintenanceService
    {
        private readonly NodeContext _nodeContext;

        public CashStatementPaymentMaintenanceService(NodeContext nodeContext)
        {
            _nodeContext = nodeContext;
        }

        public async Task<CashManagerStatementPaymentEditorState?> GetEditorAsync(
            string paymentCode,
            CancellationToken cancellationToken = default)
        {
            paymentCode = NormalizeCode(paymentCode);

            if (string.IsNullOrWhiteSpace(paymentCode))
            {
                return null;
            }

            var payment = await _nodeContext.Cash_tbPayments
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.PaymentCode == paymentCode, cancellationToken);

            if (payment is null)
            {
                return null;
            }

            var accountTypeCode = await _nodeContext.Subject_tbAccounts
                .AsNoTracking()
                .Where(item => item.AccountCode == payment.AccountCode)
                .Select(item => item.AccountTypeCode)
                .FirstOrDefaultAsync(cancellationToken);

            var periodStatus = await GetCashStatusAsync(payment.PaidOn, cancellationToken);

            var model = new CashManagerStatementPaymentEditorModel {
                PaymentCode = payment.PaymentCode,
                SubjectCode = payment.SubjectCode,
                CurrentAccountCode = payment.AccountCode,
                AccountCode = payment.AccountCode,
                UserId = payment.UserId,
                PaidOn = payment.PaidOn,
                PaymentReference = payment.PaymentReference ?? string.Empty,
                CashCode = payment.CashCode ?? string.Empty,
                TaxCode = payment.TaxCode ?? string.Empty,
                PaidInValue = payment.PaidInValue,
                PaidOutValue = payment.PaidOutValue,
                IsClosedPeriod = IsClosedPeriod(periodStatus),
                PeriodStatus = periodStatus
            };

            var isTransfer = payment.PaymentStatusCode == (short)NodeEnum.PaymentStatus.Transfer;

            return new CashManagerStatementPaymentEditorState(
                model,
                await GetUsersAsync(cancellationToken),
                isTransfer
                    ? await GetTransferCashCodesAsync(model.CashCode, cancellationToken)
                    : await GetCashCodesAsync(cancellationToken),
                await GetTaxCodesAsync(model.TaxCode, cancellationToken),
                await GetAccountsAsync(accountTypeCode, cancellationToken),
                !string.IsNullOrWhiteSpace(payment.CashCode)
                    && accountTypeCode != (short)NodeEnum.CashAccountType.Asset);
        }

        public async Task SaveEditAsync(
            CashManagerStatementPaymentEditorModel model,
            string userId,
            bool isAdministrator,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            var payment = await LoadPaymentAsync(model.PaymentCode, cancellationToken);
            var touchesClosedPeriod = await EnsureCanModifyAsync(
                payment.PaidOn,
                model.PaidOn,
                model.AllowClosedPeriodOverride,
                isAdministrator,
                cancellationToken);

            var profile = new Profile(_nodeContext);
            payment.UserId = NormalizeCode(model.UserId);
            payment.PaidOn = model.PaidOn;
            payment.PaymentReference = NormalizeText(model.PaymentReference);
            payment.CashCode = NormalizeNullableCode(model.CashCode);
            payment.UpdatedBy = await profile.UserName(userId);

            await _nodeContext.SaveChangesAsync(cancellationToken);
            await RebuildAsync(payment.SubjectCode, touchesClosedPeriod);
        }

        public async Task MoveAsync(
            CashManagerStatementPaymentEditorModel model,
            string userId,
            bool isAdministrator,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            var payment = await LoadPaymentAsync(model.PaymentCode, cancellationToken);

            await EnsureCanModifyAsync(
                payment.PaidOn,
                payment.PaidOn,
                model.AllowClosedPeriodOverride,
                isAdministrator,
                cancellationToken);

            var targetAccountCode = NormalizeCode(model.AccountCode);

            if (string.IsNullOrWhiteSpace(targetAccountCode))
            {
                throw new InvalidOperationException("A target cash account is required.");
            }

            if (string.Equals(payment.AccountCode, targetAccountCode, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var sourceAccountTypeCode = await _nodeContext.Subject_tbAccounts
                .AsNoTracking()
                .Where(item => item.AccountCode == payment.AccountCode)
                .Select(item => item.AccountTypeCode)
                .FirstOrDefaultAsync(cancellationToken);

            var targetAccountTypeCode = await _nodeContext.Subject_tbAccounts
                .AsNoTracking()
                .Where(item => item.AccountCode == targetAccountCode)
                .Select(item => (short?)item.AccountTypeCode)
                .FirstOrDefaultAsync(cancellationToken);

            if (!targetAccountTypeCode.HasValue)
            {
                throw new InvalidOperationException("The target cash account was not found.");
            }

            if (sourceAccountTypeCode != targetAccountTypeCode.Value)
            {
                throw new InvalidOperationException("Payments can only be moved to cash accounts of the same type.");
            }

            var cashAccounts = new CashAccounts(_nodeContext, targetAccountCode);

            if (!await cashAccounts.MovePayment(payment.PaymentCode))
            {
                throw new InvalidOperationException($"Payment '{payment.PaymentCode}' could not be moved.");
            }
        }

        public async Task SavePaymentAsync(
            CashManagerStatementPaymentEditorModel model,
            string userId,
            bool isAdministrator,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            var payment = await LoadPaymentAsync(model.PaymentCode, cancellationToken);
            var touchesClosedPeriod = await EnsureCanModifyAsync(
                payment.PaidOn,
                payment.PaidOn,
                model.AllowClosedPeriodOverride,
                isAdministrator,
                cancellationToken);

            var profile = new Profile(_nodeContext);
            payment.PaidInValue = model.PaidInValue;
            payment.PaidOutValue = model.PaidOutValue;
            payment.TaxCode = NormalizeNullableCode(model.TaxCode);
            payment.UpdatedBy = await profile.UserName(userId);

            await _nodeContext.SaveChangesAsync(cancellationToken);
            await RebuildAsync(payment.SubjectCode, touchesClosedPeriod);
        }

        public async Task DeleteAsync(
            CashManagerStatementPaymentEditorModel model,
            string userId,
            bool isAdministrator,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            var payment = await LoadPaymentAsync(model.PaymentCode, cancellationToken);

            await EnsureCanModifyAsync(
                payment.PaidOn,
                payment.PaidOn,
                model.AllowClosedPeriodOverride,
                isAdministrator,
                cancellationToken);

            var cashAccounts = new CashAccounts(_nodeContext);

            if (!await cashAccounts.DeletePayment(payment.PaymentCode))
            {
                throw new InvalidOperationException($"Payment '{payment.PaymentCode}' could not be deleted.");
            }
        }

        private async Task<Cash_tbPayment> LoadPaymentAsync(
            string paymentCode,
            CancellationToken cancellationToken)
        {
            paymentCode = NormalizeCode(paymentCode);

            var payment = await _nodeContext.Cash_tbPayments
                .FirstOrDefaultAsync(item => item.PaymentCode == paymentCode, cancellationToken);

            return payment ?? throw new InvalidOperationException($"Payment '{paymentCode}' was not found.");
        }

        private async Task<bool> EnsureCanModifyAsync(
            DateTime currentPaidOn,
            DateTime targetPaidOn,
            bool allowClosedPeriodOverride,
            bool isAdministrator,
            CancellationToken cancellationToken)
        {
            var currentStatus = await GetCashStatusAsync(currentPaidOn, cancellationToken);
            var targetStatus = await GetCashStatusAsync(targetPaidOn, cancellationToken);
            var touchesClosedPeriod = IsClosedPeriod(currentStatus) || IsClosedPeriod(targetStatus);

            if (!touchesClosedPeriod)
            {
                return false;
            }

            if (!isAdministrator)
            {
                throw new InvalidOperationException("Closed-period corrections are restricted to Administrators.");
            }

            if (!allowClosedPeriodOverride)
            {
                throw new InvalidOperationException("Closed-period correction requires confirmation before saving.");
            }

            return true;
        }

        private async Task<NodeEnum.CashStatus> GetCashStatusAsync(
            DateTime paidOn,
            CancellationToken cancellationToken)
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
            var options = await _nodeContext.Cash_tbCodes
                .AsNoTracking()
                .OrderBy(item => item.CashDescription)
                .Select(item => new CashManagerSelectOption(
                    item.CashCode,
                    item.CashDescription))
                .ToListAsync(cancellationToken);

            options.Insert(0, new CashManagerSelectOption(string.Empty, string.Empty));
            return options;
        }

        private async Task<IReadOnlyList<CashManagerSelectOption>> GetTransferCashCodesAsync(
            string selectedCashCode,
            CancellationToken cancellationToken)
        {
            var options = await _nodeContext.Cash_TransferCodeLookup
                .AsNoTracking()
                .OrderBy(item => item.CashPolarityCode)
                .ThenBy(item => item.CashDescription)
                .Select(item => new CashManagerSelectOption(
                    item.CashCode,
                    item.CashDescription))
                .ToListAsync(cancellationToken);

            options.Insert(0, new CashManagerSelectOption(string.Empty, string.Empty));

            if (string.IsNullOrWhiteSpace(selectedCashCode)
                || options.Any(item => string.Equals(item.Value, selectedCashCode, StringComparison.OrdinalIgnoreCase)))
            {
                return options;
            }

            options.Insert(1, new CashManagerSelectOption(selectedCashCode, selectedCashCode));
            return options;
        }

        private async Task<IReadOnlyList<CashManagerSelectOption>> GetTaxCodesAsync(
            string selectedTaxCode,
            CancellationToken cancellationToken)
        {
            var options = await _nodeContext.App_tbTaxCodes
                .AsNoTracking()
                .OrderBy(item => item.TaxDescription)
                .Select(item => new CashManagerSelectOption(
                    item.TaxCode,
                    item.TaxDescription))
                .ToListAsync(cancellationToken);

            options.Insert(0, new CashManagerSelectOption(string.Empty, string.Empty));

            if (string.IsNullOrWhiteSpace(selectedTaxCode)
                || options.Any(item => string.Equals(item.Value, selectedTaxCode, StringComparison.OrdinalIgnoreCase)))
            {
                return options;
            }

            options.Insert(1, new CashManagerSelectOption(selectedTaxCode, selectedTaxCode));
            return options;
        }

        private async Task<IReadOnlyList<CashManagerSelectOption>> GetAccountsAsync(
            short accountTypeCode,
            CancellationToken cancellationToken)
        {
            return await _nodeContext.Set<Subject_vwCashAccount>()
                .AsNoTracking()
                .Where(item => item.AccountTypeCode == accountTypeCode)
                .OrderBy(item => item.AccountName)
                .ThenBy(item => item.AccountCode)
                .Select(item => new CashManagerSelectOption(
                    item.AccountCode,
                    $"{item.AccountName} ({item.AccountCode})"))
                .ToListAsync(cancellationToken);
        }

        private async Task RebuildAsync(string subjectCode, bool touchesClosedPeriod)
        {
            if (touchesClosedPeriod)
            {
                var cashAccounts = new CashAccounts(_nodeContext);
                await cashAccounts.RebuildAccount();
                return;
            }

            var subjects = new Subjects(_nodeContext, subjectCode);
            await subjects.Rebuild();
        }

        private static bool IsClosedPeriod(NodeEnum.CashStatus status)
        {
            return status == NodeEnum.CashStatus.Closed
                || status == NodeEnum.CashStatus.Archived;
        }

        private static string NormalizeCode(string? value)
        {
            return value?.Trim() ?? string.Empty;
        }

        private static string NormalizeText(string? value)
        {
            return value?.Trim() ?? string.Empty;
        }

        private static string? NormalizeNullableCode(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
