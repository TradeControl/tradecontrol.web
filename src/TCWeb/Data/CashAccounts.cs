using System;
using System.Data;
using Microsoft.Data.SqlClient;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TradeControl.Web.Data
{
    public class CashAccounts
    {
        readonly NodeContext _context;

        public string CashAccountCode { get; } = string.Empty;


        public CashAccounts(NodeContext context) 
        {
            _context = context;
        }

        public CashAccounts(NodeContext context, string cashAccount)
        {
            _context = context;
            CashAccountCode = cashAccount;
        }


        public async Task<bool> PostPayment(string userId)
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Cash.proc_PaymentPostById @p0", parameters: new[] { userId });

                return result != 0;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return false;
            }
            
        }

        public async Task<bool> PostTransfer(string paymentCode)
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Cash.proc_PayAccrual @p0", parameters: new[] { paymentCode });

                return result != 0;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return false;
            }
        }



        public async Task<bool> RebuildAccount()
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Cash.proc_AccountRebuild");

                return result != 0;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<bool> RebuildSystem()
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("App.proc_SystemRebuild");

                return result != 0;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return false;
            }
        }


        public async Task<bool> MovePayment(string paymentCode)
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Cash.proc_PaymentMove @p0, @p1", parameters: new[] { paymentCode, CashAccountCode });

                return result != 0;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<bool> DeletePayment(string paymentCode)
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Cash.proc_PaymentDelete @p0", parameters: new[] { paymentCode });

                return result != 0;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<bool> PostAsset(string paymentCode) => await _context.PostAsset(paymentCode);

        public async Task<string> NextPaymentCode() => await _context.NextPaymentCode();

        public async Task<string> AddPayment(string accountCode, string cashCode, DateTime paidOn, decimal toPay) => await _context.AddPayment(CashAccountCode, accountCode, cashCode, paidOn, toPay);


        public async Task<string> CurrentAccount() => await _context.CurrentAccount;

        public async Task<string> ReserveAccount() => await _context.ReserveAccount;

        public async Task<NodeEnum.CoinType> CoinType() => await _context.CoinType;

    }
}
