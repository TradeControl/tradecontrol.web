using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Models;

namespace TradeControl.Web.Data
{
    public class CashCodes
    {
        readonly NodeContext _context;
        Cash_proc_CodeDefaults _cashCode = new();

        public CashCodes(NodeContext context)
        {
            _context = context;
        }

        public CashCodes(NodeContext context, string cashCode)
        {
            _context = context;
            CashCode = cashCode;
        }

        #region properties
        public string CashCode
        {
            get
            {
                return _cashCode.CashCode;
            }
            set
            {
                try
                {
                    var _cashCode = new SqlParameter()
                    {
                        ParameterName = "@CashCode",
                        SqlDbType = System.Data.SqlDbType.VarChar,
                        Direction = System.Data.ParameterDirection.Input,
                        Size = 40,
                        Value = value
                    };

                    string sql = $"Cash.proc_CodeDefaults @CashCode";

                    var results = _context.Cash_CodeDefaults.FromSqlRaw(sql, _cashCode).ToList();

                    if (results != null)
                        this._cashCode = results.Select(t => t).FirstOrDefault();
                }
                catch (Exception e)
                {
                    _ = _context.ErrorLog(e);
                }
                
            }
        }

        public string Description
        {
            get { return _cashCode.CashDescription;  }
        }

        public string CategoryCode
        {
            get { return _cashCode.CategoryCode;  }
        }

        public string TaxCode
        {
            get { return _cashCode.TaxCode; }
        }

        public NodeEnum.TaxType TaxTypeCode
        {
            get { return _cashCode.TaxTypeCode; }
        }

        public NodeEnum.CashPolarity CashPolarityCode
        {
            get { return _cashCode.CashPolarityCode;  }
        }

        public NodeEnum.CashType CashTypeCode
        {
            get { return _cashCode.CashTypeCode; }
        }

        #endregion

        #region settings
        public Task<string> GetTaxCashCode(NodeEnum.TaxType taxType)
        {
            return Task.Run(() =>
            {
                try
                {
                    return _context.Cash_tbTaxTypes.Where(t => t.TaxTypeCode == (short)taxType).Select(t => t.CashCode).ToString();
                }
                catch (Exception e)
                {
                    _ = _context.ErrorLog(e);
                    return string.Empty;
                }
            });
        }

        public async Task<bool> IsVatCashCode() => CashCode == await GetTaxCashCode(NodeEnum.TaxType.VAT);        

        public bool IsTransfer
        {
            get { return CashTypeCode == NodeEnum.CashType.Bank; }
        }

        public async Task<decimal> GetVatBalance() => await _context.VatBalance();
        #endregion

        #region actions
        public async Task<bool> MirrorChargeCode(string accountCode, string chargeCode)
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Cash.proc_PayAccrual @p0, @p1, @p2", parameters: new[] { CashCode, accountCode, chargeCode });

                return result != 0;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }
        #endregion
    }
}
