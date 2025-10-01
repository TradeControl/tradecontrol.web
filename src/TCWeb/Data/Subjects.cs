using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TradeControl.Web.Data
{
    public class Subjects
    {
        readonly NodeContext _context;

        public string AccountCode { get; } = string.Empty;

        public Subjects(NodeContext context)
        {
            _context = context;
        }

        public Subjects(NodeContext context, string accountCode)
        {
            _context = context;
            AccountCode = accountCode;
        }

        #region properties
        public async Task<string> AddressCode()
        {
            try
            {
                return await _context.Subject_tbSubjects.Where(o => o.AccountCode == AccountCode).Select(o => o.AddressCode).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<decimal> BalanceOutstanding() => await _context.BalanceOutstanding(AccountCode);

        public async Task<decimal> BalanceToPay() =>await _context.BalanceToPay(AccountCode);
        #endregion

        #region methods
        public async Task<bool> Rebuild()
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Subject.proc_Rebuild @p0", parameters: new[] { AccountCode });
                return result != 0;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }

        }
        public async Task AddContact(string contactName)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("Subject.proc_AddContact @p0, @p1", parameters: new[] { AccountCode, contactName });
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
            }
        }

        public async Task AddAddress(string address)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("Subject.proc_AddAddress @p0, @p1", parameters: new[] { AccountCode, address });
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
            }
        }

        public async Task<string> NextAddressCode() => await _context.NextAddressCode(AccountCode);

        public async Task<string> DefaultAccountCode(string accountName) => await _context.SubjectAccountCodeDefault(accountName);

        public async Task<string> DefaultTaxCode() => await _context.SubjectTaxCodeDefault(AccountCode);

        public async Task<string> DefaultEmailAddress() => await _context.SubjectEmailAddressDefault(AccountCode);

        #endregion

    }
}
