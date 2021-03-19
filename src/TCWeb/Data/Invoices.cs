﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TradeControl.Web.Data
{
    public class Invoices
    {
        NodeContext _context;

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
                _context.ErrorLog(e);
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
                _context.ErrorLog(e);
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
                _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<bool> CancelPending()
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Invoice.proc_Cancel");
                return result != 0;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
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

        public async Task<bool> Post()
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Invoice.proc_PostEntries");
                return result != 0;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return false;
            }
        }
        #endregion
    }
}