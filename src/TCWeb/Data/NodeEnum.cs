using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeControl.Web.Data
{
    public class NodeEnum
    {
        public enum OpStatus { Pending, InProgress, Complete }
        public enum OrgStatus { Pending, Active, Hot, Dead };
        public enum CategoryType { CashCode, CashTotal, Expression };
        public enum CashMode { Expense, Income, Neutral };
        public enum CashType { Trade, Tax, Bank };
        public enum TaskStatus { Pending, Opened, Closed, Charged, Cancelled, Archived };
        public enum DocType { Quotation, SalesOrder, Enquiry, PurchaseOrder, SalesInvoice, CreditNote, DebitNote };
        public enum DocClassCode { Product, Money };
        public enum InvoiceType { SalesInvoice, CreditNote, PurchaseInvoice, DebitNote };
        public enum InvoiceStatus { Pending, Invoiced, PartiallyPaid, Paid };
        public enum TaxType { CorporationTax, VAT, NI, General };
        public enum CashEntryType { Payment, Invoice, Order, Quote, CorporationTax, VAT, Forecast, Transfers };
        public enum EventType { IsError, IsWarning, IsInformation, IsPriceChange, IsReschedule, IsDelivered, IsStatusChange, IsPayment };
        public enum TransmitStatus { IsDisconnected, IsDeploy, IsUpdate, IsProcessed };
        public enum CoinType { MainNet, TestNet, Fiat };
        public enum PaymentStatus { Unposted, Posted, Transfer };
        public enum InterfaceCode { Accounts, MIS };
        public enum RoundingCode { Round, Truncate };

    }
}
