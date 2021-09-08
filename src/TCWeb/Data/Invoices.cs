using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

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
        public async Task<bool> Raise(string taskCode, NodeEnum.InvoiceType invoiceType, DateTime invoicedOn)
        {
            InvoiceNumber = await _context.InvoiceRaise(taskCode, invoiceType, invoicedOn);
            return InvoiceNumber.Length > 0;
        }

        public async Task<bool> RaiseBlank(string accountCode, NodeEnum.InvoiceType invoiceType)
        {
            InvoiceNumber = await _context.InvoiceRaiseBlank(accountCode, invoiceType);
            return InvoiceNumber.Length > 0;
        }

        public async Task<bool> Credit()
        {
            InvoiceNumber = await _context.InvoiceCredit(InvoiceNumber);
            return InvoiceNumber.Length > 0;
        }

        public async Task<bool> AddTask(string taskCode)
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Invoice.proc_AddTask @p0, @p1", parameters: new[] { InvoiceNumber, taskCode });
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
                int result = await _context.Database.ExecuteSqlRawAsync("Invoice.proc_CancelById @p0", userId);
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
                int result = await _context.Database.ExecuteSqlRawAsync("Invoice.proc_PostEntriesById @p0", parameters: new[] { userId });
                return result != 0;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<bool> PostByEntry(string userId, string accountCode, string cashCode)
        {
            try
            {
                var _userId = new SqlParameter()
                {
                    ParameterName = "@UserId",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 10,
                    Value = userId
                };

                var _accountCode = new SqlParameter()
                {
                    ParameterName = "@AccountCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 10,
                    Value = accountCode
                };

                var _cashCode = new SqlParameter()
                {
                    ParameterName = "@CashCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 50,
                    Value = cashCode
                };

                using (SqlConnection _connection = new(_context.Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Invoice.proc_PostEntryById";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_userId); 
                        _command.Parameters.Add(_accountCode);
                        _command.Parameters.Add(_cashCode);

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

        public async Task<bool> PostByAccount(string userId, string accountCode)
        {
            try
            {
                var _userId = new SqlParameter()
                {
                    ParameterName = "@UserId",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 10,
                    Value = userId
                };

                var _accountCode = new SqlParameter()
                {
                    ParameterName = "@AccountCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 10,
                    Value = accountCode
                };

                using (SqlConnection _connection = new(_context.Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Invoice.proc_PostAccountById";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_userId);
                        _command.Parameters.Add(_accountCode);

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
                var invoice = await _context.Invoice_tbInvoices.Where(i => i.InvoiceNumber == InvoiceNumber).SingleOrDefaultAsync();
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
