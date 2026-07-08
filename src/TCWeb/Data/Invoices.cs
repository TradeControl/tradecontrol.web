using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Pages.Invoice.Register.Models;

namespace TradeControl.Web.Data
{
    public class Invoices
    {
        readonly NodeContext _context;

        public string InvoiceNumber { get; private set; } = string.Empty;

        public Invoices(NodeContext context)
        {
            _context = context;
        }

        public Invoices(NodeContext context, string invoiceNumber)
        {
            _context = context;
            InvoiceNumber = invoiceNumber;
        }

        #region methods
        public async Task<bool> Raise(string projectCode, NodeEnum.InvoiceType invoiceType, DateTime invoicedOn)
        {
            InvoiceNumber = await _context.InvoiceRaise(projectCode, invoiceType, invoicedOn);
            return InvoiceNumber.Length > 0;
        }

        public async Task<bool> RaiseBlank(string subjectCode, NodeEnum.InvoiceType invoiceType, string? parentSubjectCode = null)
        {
            InvoiceNumber = await _context.InvoiceRaiseBlank(subjectCode, invoiceType, parentSubjectCode);
            return InvoiceNumber.Length > 0;
        }

        public async Task<bool> Credit()
        {
            InvoiceNumber = await _context.InvoiceCredit(InvoiceNumber);
            return InvoiceNumber.Length > 0;
        }

        public async Task<string> DefaultEntryCode(string userId)
        {
            try
            {
                var _userId = new SqlParameter()
                {
                    ParameterName = "@UserId",
                    SqlDbType = SqlDbType.VarChar,
                    Direction = ParameterDirection.Input,
                    Size = 10,
                    Value = userId
                };

                var _entryId = new SqlParameter()
                {
                    ParameterName = "@EntryId",
                    SqlDbType = SqlDbType.NVarChar,
                    Direction = ParameterDirection.Output,
                    Size = 50
                };

                using (SqlConnection _connection = new(_context.Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Invoice.proc_DefaultEntryCode";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_userId);
                        _command.Parameters.Add(_entryId);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return _entryId.Value?.ToString() ?? string.Empty;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<InvoiceRaiseDefaultsModel?> RaiseDefaults(string subjectCode, string? parentSubjectCode = null, string? entryId = null, string? cashCode = null)
        {
            try
            {
                var defaults = new InvoiceRaiseDefaultsModel();

                var _subjectCode = new SqlParameter()
                {
                    ParameterName = "@SubjectCode",
                    SqlDbType = SqlDbType.NVarChar,
                    Direction = ParameterDirection.Input,
                    Size = 50,
                    Value = subjectCode
                };

                var _parentSubjectCode = new SqlParameter()
                {
                    ParameterName = "@ParentSubjectCode",
                    SqlDbType = SqlDbType.NVarChar,
                    Direction = ParameterDirection.Input,
                    Size = 50,
                    Value = string.IsNullOrWhiteSpace(parentSubjectCode)
                        ? DBNull.Value
                        : parentSubjectCode.Trim()
                };

                var _entryId = new SqlParameter()
                {
                    ParameterName = "@EntryId",
                    SqlDbType = SqlDbType.NVarChar,
                    Direction = ParameterDirection.Input,
                    Size = 20,
                    Value = string.IsNullOrWhiteSpace(entryId)
                        ? DBNull.Value
                        : entryId.Trim()
                };

                var _cashCode = new SqlParameter()
                {
                    ParameterName = "@CashCode",
                    SqlDbType = SqlDbType.NVarChar,
                    Direction = ParameterDirection.Input,
                    Size = 50,
                    Value = string.IsNullOrWhiteSpace(cashCode)
                        ? DBNull.Value
                        : cashCode.Trim()
                };

                using (SqlConnection _connection = new(_context.Database.GetConnectionString()))
                {
                    await _connection.OpenAsync();

                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Invoice.proc_RaiseDefaults";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_subjectCode);
                        _command.Parameters.Add(_parentSubjectCode);
                        _command.Parameters.Add(_entryId);
                        _command.Parameters.Add(_cashCode);

                        using var reader = await _command.ExecuteReaderAsync();

                        if (!await reader.ReadAsync())
                            return null;

                        defaults.SubjectCode = reader["SubjectCode"]?.ToString() ?? string.Empty;
                        defaults.ParentSubjectCode = reader["ParentSubjectCode"]?.ToString() ?? string.Empty;
                        defaults.TaxCode = reader["TaxCode"]?.ToString() ?? string.Empty;
                        defaults.InvoiceTypeCode = reader["InvoiceTypeCode"] == DBNull.Value ? (short)0 : Convert.ToInt16(reader["InvoiceTypeCode"]);
                        defaults.CashCode = reader["CashCode"]?.ToString() ?? string.Empty;
                        defaults.TotalValue = reader["TotalValue"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["TotalValue"]);
                        defaults.InvoiceValue = reader["InvoiceValue"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["InvoiceValue"]);
                        defaults.ItemReference = reader["ItemReference"]?.ToString() ?? string.Empty;
                    }

                    await _connection.CloseAsync();
                }

                return defaults;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return null;
            }
        }

        public async Task<bool> AddProject(string projectCode)
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Invoice.proc_AddProject @p0, @p1", parameters: new[] { InvoiceNumber, projectCode });
                return result != 0;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<bool> Accept()
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Invoice.proc_Accept @p0", parameters: new[] { InvoiceNumber });
                return result != 0;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }

        /// <summary>
        /// Pay outstanding amount
        /// </summary>
        /// <returns>Payment Code</returns>
        public async Task<string> Pay(DateTime paidOn, bool postPayment)
        {
            return await _context.InvoicePay(InvoiceNumber, paidOn, postPayment);
        }

        public async Task<bool> Recalculate()
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Invoice.proc_Total @p0", parameters: new[] { InvoiceNumber });
                return result != 0;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<bool> CancelPending(string userId)
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Invoice.proc_CancelByUserId @p0", userId);
                return result != 0;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<NodeEnum.DocType> DefaultDocType() => await _context.InvoiceDefaultDocType(InvoiceNumber);

        public async Task<DateTime> DefaultPaymentOn(string accountCode, DateTime actionOn) => await _context.InvoiceDefaultPaymentOn(accountCode, actionOn);

        public async Task<bool> Mirror(string contractAddress)
        {
            InvoiceNumber = await _context.InvoiceMirror(contractAddress);
            return InvoiceNumber.Length > 0;
        }

        public async Task<bool> Post(string userId)
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Invoice.proc_PostEntriesByUserId @p0", parameters: new[] { userId });
                return result != 0;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<bool> PostByEntry(string userId, string entryId, string? parentSubjectCode = null)
        {
            try
            {
                var _userId = new SqlParameter()
                {
                    ParameterName = "@UserId",
                    SqlDbType = SqlDbType.VarChar,
                    Direction = ParameterDirection.Input,
                    Size = 10,
                    Value = userId
                };

                var _entryId = new SqlParameter()
                {
                    ParameterName = "@EntryId",
                    SqlDbType = SqlDbType.NVarChar,
                    Direction = ParameterDirection.Input,
                    Size = 20,
                    Value = entryId
                };

                var _parentSubjectCode = new SqlParameter()
                {
                    ParameterName = "@ParentSubjectCode",
                    SqlDbType = SqlDbType.VarChar,
                    Direction = ParameterDirection.Input,
                    Size = 50,
                    Value = string.IsNullOrWhiteSpace(parentSubjectCode)
                        ? DBNull.Value
                        : parentSubjectCode.Trim()
                };

                using (SqlConnection _connection = new(_context.Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Invoice.proc_PostEntryByUserId";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_userId);
                        _command.Parameters.Add(_entryId);
                        _command.Parameters.Add(_parentSubjectCode);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return true;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<bool> PostByAccount(string userId, string accountCode, string? parentSubjectCode = null)
        {
            try
            {
                var _userId = new SqlParameter()
                {
                    ParameterName = "@UserId",
                    SqlDbType = SqlDbType.VarChar,
                    Direction = ParameterDirection.Input,
                    Size = 10,
                    Value = userId
                };

                var _accountCode = new SqlParameter()
                {
                    ParameterName = "@SubjectCode",
                    SqlDbType = SqlDbType.VarChar,
                    Direction = ParameterDirection.Input,
                    Size = 50,
                    Value = accountCode
                };

                var _parentSubjectCode = new SqlParameter()
                {
                    ParameterName = "@ParentSubjectCode",
                    SqlDbType = SqlDbType.VarChar,
                    Direction = ParameterDirection.Input,
                    Size = 50,
                    Value = string.IsNullOrWhiteSpace(parentSubjectCode)
                        ? DBNull.Value
                        : parentSubjectCode.Trim()
                };

                using (SqlConnection _connection = new(_context.Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Invoice.proc_PostAccountByUserId";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_userId);
                        _command.Parameters.Add(_accountCode);
                        _command.Parameters.Add(_parentSubjectCode);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return true;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }

        public async Task SetToPrinted()
        {
            try
            {
                var invoice = await _context.Invoice_tbInvoices
                    .Where(i => i.InvoiceNumber == InvoiceNumber)
                    .SingleOrDefaultAsync();

                if (invoice != null)
                {
                    invoice.Spooled = false;
                    invoice.Printed = true;
                    _context.Attach(invoice).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
            }
        }
        #endregion
    }
}
