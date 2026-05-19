using System;
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
    public sealed class CashAccountMaintenanceService : ICashAccountMaintenanceService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public CashAccountMaintenanceService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<CashAccountEditorOptions> GetEditorOptionsAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var organisations = await nodeContext.Subject_SubjectLookup
                .AsNoTracking()
                .OrderBy(item => item.SubjectName)
                .Select(item => new CashAccountLookupOption(
                    item.SubjectName,
                    item.SubjectName))
                .ToListAsync(cancellationToken);

            var accountTypes = await nodeContext.Subject_tbAccountTypes
                .AsNoTracking()
                .OrderBy(item => item.AccountTypeCode)
                .Select(item => new CashAccountLookupOption(
                    item.AccountType,
                    item.AccountType))
                .ToListAsync(cancellationToken);

            var cashCodes = await nodeContext.Cash_BankCashCodes
                .AsNoTracking()
                .OrderBy(item => item.CashDescription)
                .Select(item => new CashAccountLookupOption(
                    item.CashDescription,
                    item.CashDescription))
                .ToListAsync(cancellationToken);

            cashCodes.Add(new CashAccountLookupOption(string.Empty, string.Empty));

            var balanceConstraints = await nodeContext.Subject_tbBalanceConstraints
                .AsNoTracking()
                .OrderBy(item => item.BalanceConstraintCode)
                .Select(item => new CashAccountBalanceConstraintOption(
                    item.BalanceConstraintCode,
                    item.BalanceConstraint))
                .ToListAsync(cancellationToken);

            return new CashAccountEditorOptions(
                organisations,
                accountTypes,
                cashCodes,
                balanceConstraints);
        }

        public async Task<CashAccountEditorModel> CreateEditorAsync(
            string? preferredAccountType,
            CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var cashAccounts = new CashAccounts(nodeContext);
            var settings = new NodeSettings(nodeContext);

            var homeOrganisation = await nodeContext.App_HomeAccount
                .AsNoTracking()
                .Select(item => item.SubjectName)
                .FirstAsync(cancellationToken);

            var accountTypeCode = await nodeContext.Subject_tbAccountTypes
                .AsNoTracking()
                .Where(item => item.AccountType == preferredAccountType)
                .Select(item => (short?)item.AccountTypeCode)
                .FirstOrDefaultAsync(cancellationToken) ?? 0;

            return new CashAccountEditorModel {
                IsNew = true,
                OriginalAccountCode = string.Empty,
                AccountCode = await cashAccounts.CurrentAccount(),
                OrganisationName = homeOrganisation ?? string.Empty,
                AccountType = preferredAccountType ?? nameof(NodeEnum.CashAccountType.Cash),
                AccountTypeCode = accountTypeCode,
                CoinTypeCode = (short)await settings.CoinType,
                LiquidityLevel = 0,
                OpeningBalance = 0m,
                CurrentBalance = 0m,
                BalanceConstraintCode = 0,
                AccountClosed = false
            };
        }

        public async Task<CashAccountEditorModel?> GetEditorAsync(
            string accountCode,
            CancellationToken cancellationToken = default)
        {
            accountCode = NormalizeCode(accountCode);

            if (string.IsNullOrWhiteSpace(accountCode))
            {
                return null;
            }

            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var account = await nodeContext.Set<Subject_vwCashAccount>()
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.AccountCode == accountCode, cancellationToken);

            return account is null
                ? null
                : CashManagerSectionCatalog.CreateEditor(account);
        }

        public async Task<Subject_vwCashAccount?> GetDetailsAsync(
            string accountCode,
            CancellationToken cancellationToken = default)
        {
            accountCode = NormalizeCode(accountCode);

            if (string.IsNullOrWhiteSpace(accountCode))
            {
                return null;
            }

            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            return await nodeContext.Set<Subject_vwCashAccount>()
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.AccountCode == accountCode, cancellationToken);
        }

        public async Task<string> SaveAsync(
            CashAccountEditorModel model,
            string userId,
            CancellationToken cancellationToken = default)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            model.AccountCode = NormalizeCode(model.AccountCode);
            model.OriginalAccountCode = NormalizeCode(model.OriginalAccountCode);

            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var profile = new Profile(nodeContext);
            var userName = await profile.UserName(userId);

            var subjectCode = await nodeContext.Subject_tbSubjects
                .AsNoTracking()
                .Where(item => item.SubjectName == model.OrganisationName)
                .Select(item => item.SubjectCode)
                .FirstAsync(cancellationToken);

            var accountTypeCode = await nodeContext.Subject_tbAccountTypes
                .AsNoTracking()
                .Where(item => item.AccountType == model.AccountType)
                .Select(item => item.AccountTypeCode)
                .FirstAsync(cancellationToken);

            var cashCode = string.IsNullOrWhiteSpace(model.CashDescription)
                ? null
                : await nodeContext.Cash_tbCodes
                    .AsNoTracking()
                    .Where(item => item.CashDescription == model.CashDescription)
                    .Select(item => item.CashCode)
                    .FirstAsync(cancellationToken);

            if (model.IsNew)
            {
                var entity = new Subject_tbAccount {
                    AccountCode = model.AccountCode,
                    SubjectCode = subjectCode,
                    AccountName = model.AccountName,
                    SortCode = model.SortCode,
                    AccountNumber = model.AccountNumber,
                    CashCode = cashCode,
                    AccountClosed = model.AccountClosed,
                    InsertedBy = userName,
                    UpdatedBy = userName,
                    OpeningBalance = model.OpeningBalance,
                    CurrentBalance = model.OpeningBalance,
                    CoinTypeCode = model.CoinTypeCode,
                    AccountTypeCode = accountTypeCode,
                    BalanceConstraintCode = model.BalanceConstraintCode,
                    LiquidityLevel = model.LiquidityLevel
                };

                nodeContext.Subject_tbAccounts.Add(entity);
                await nodeContext.SaveChangesAsync(cancellationToken);
                return entity.AccountCode;
            }

            if (!string.Equals(model.OriginalAccountCode, model.AccountCode, StringComparison.Ordinal))
            {
                var rowsAffected = await nodeContext.Database.ExecuteSqlInterpolatedAsync(
                    $@"UPDATE Subject.tbAccount
                       SET AccountCode = {model.AccountCode},
                           SubjectCode = {subjectCode},
                           AccountName = {model.AccountName},
                           SortCode = {model.SortCode},
                           AccountNumber = {model.AccountNumber},
                           CashCode = {cashCode},
                           AccountClosed = {model.AccountClosed},
                           OpeningBalance = {model.OpeningBalance},
                           CoinTypeCode = {model.CoinTypeCode},
                           AccountTypeCode = {accountTypeCode},
                           BalanceConstraintCode = {model.BalanceConstraintCode},
                           LiquidityLevel = {model.LiquidityLevel}
                       WHERE AccountCode = {model.OriginalAccountCode}",
                    cancellationToken);

                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException($"Cash account '{model.OriginalAccountCode}' was not updated.");
                }

                return model.AccountCode;
            }

            var accountEntity = await nodeContext.Subject_tbAccounts
                .FirstAsync(item => item.AccountCode == model.OriginalAccountCode, cancellationToken);

            accountEntity.SubjectCode = subjectCode;
            accountEntity.AccountName = model.AccountName;
            accountEntity.SortCode = model.SortCode;
            accountEntity.AccountNumber = model.AccountNumber;
            accountEntity.CashCode = cashCode;
            accountEntity.AccountClosed = model.AccountClosed;
            accountEntity.OpeningBalance = model.OpeningBalance;
            accountEntity.CoinTypeCode = model.CoinTypeCode;
            accountEntity.AccountTypeCode = accountTypeCode;
            accountEntity.BalanceConstraintCode = model.BalanceConstraintCode;
            accountEntity.LiquidityLevel = model.LiquidityLevel;

            await nodeContext.SaveChangesAsync(cancellationToken);
            return accountEntity.AccountCode;
        }

        public async Task DeleteAsync(
            string accountCode,
            CancellationToken cancellationToken = default)
        {
            accountCode = NormalizeCode(accountCode);

            if (string.IsNullOrWhiteSpace(accountCode))
            {
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var hasFinancialActivity = await nodeContext.Set<Cash_tbPayment>()
                .AsNoTracking()
                .AnyAsync(payment => payment.AccountCode == accountCode, cancellationToken);

            if (hasFinancialActivity)
            {
                throw new InvalidOperationException(
                    "Delete is not allowed because there is associated financial activity with this account.");
            }

            var cashAccount = await nodeContext.Subject_tbAccounts
                .FirstOrDefaultAsync(item => item.AccountCode == accountCode, cancellationToken);

            if (cashAccount is null)
            {
                return;
            }

            nodeContext.Subject_tbAccounts.Remove(cashAccount);
            await nodeContext.SaveChangesAsync(cancellationToken);
        }

        private static string NormalizeCode(string? accountCode)
        {
            return accountCode?.Trim() ?? string.Empty;
        }
    }
}
