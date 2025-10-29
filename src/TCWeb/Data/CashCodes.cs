using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data; 

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

        public string Description => _cashCode.CashDescription;
        public string CategoryCode => _cashCode.CategoryCode;
        public string TaxCode => _cashCode.TaxCode;
        public NodeEnum.TaxType TaxTypeCode => _cashCode.TaxTypeCode;
        public NodeEnum.CashPolarity CashPolarityCode => _cashCode.CashPolarityCode;
        public NodeEnum.CashType CashTypeCode => _cashCode.CashTypeCode;
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

        public bool IsTransfer => CashTypeCode == NodeEnum.CashType.Bank;

        public async Task<decimal> GetVatBalance() => await _context.VatBalance();

        public async Task<string> GetCategoryNamespace(string categoryCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(categoryCode))
                    return string.Empty;

                var conn = _context.Database.GetDbConnection();
                var close = conn.State != ConnectionState.Open;
                if (close) await conn.OpenAsync();
                try
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT Cash.fnCategoryNamespace(@CategoryCode)";
                    var p = cmd.CreateParameter();
                    p.ParameterName = "@CategoryCode";
                    p.DbType = DbType.String;
                    p.Size = 10;
                    p.Value = categoryCode;
                    cmd.Parameters.Add(p);

                    var scalar = await cmd.ExecuteScalarAsync();
                    return scalar?.ToString() ?? string.Empty;
                }
                finally
                {
                    if (close) await conn.CloseAsync();
                }
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<string> GetCategoryNamespace(string categoryCode, string parentCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(categoryCode))
                    return string.Empty;

                if (string.IsNullOrWhiteSpace(parentCode))
                    return await GetCategoryNamespace(categoryCode);

                var conn = _context.Database.GetDbConnection();
                var close = conn.State != ConnectionState.Open;
                if (close) await conn.OpenAsync();
                try
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT Cash.fnCategoryNamespaceInContext(@CategoryCode, @ParentCode)";
                    var p1 = cmd.CreateParameter();
                    p1.ParameterName = "@CategoryCode";
                    p1.DbType = DbType.String; p1.Size = 10; p1.Value = categoryCode;
                    var p2 = cmd.CreateParameter();
                    p2.ParameterName = "@ParentCode";
                    p2.DbType = DbType.String; p2.Size = 10; p2.Value = parentCode;
                    cmd.Parameters.Add(p1); cmd.Parameters.Add(p2);

                    var scalar = await cmd.ExecuteScalarAsync();
                    return scalar?.ToString() ?? string.Empty;
                }
                finally
                {
                    if (close) await conn.CloseAsync();
                }
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return string.Empty;
            }
        }

        // Primary (NetProfit/VAT roots) namespace helper
        public async Task<string> GetPrimaryCategoryNamespace(string categoryCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(categoryCode))
                    return string.Empty;

                var conn = _context.Database.GetDbConnection();
                var close = conn.State != ConnectionState.Open;
                if (close) await conn.OpenAsync();
                try
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT Cash.fnCategoryNamespacePrimary(@CategoryCode)";
                    var p = cmd.CreateParameter();
                    p.ParameterName = "@CategoryCode";
                    p.DbType = DbType.String;
                    p.Size = 10;
                    p.Value = categoryCode;
                    cmd.Parameters.Add(p);

                    var scalar = await cmd.ExecuteScalarAsync();
                    return scalar?.ToString() ?? string.Empty;
                }
                finally
                {
                    if (close) await conn.CloseAsync();
                }
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return string.Empty;
            }
        }
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
