using System.Collections.Generic;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Invoice.Register.Models
{
    public sealed class InvoiceRegisterResult
    {
        public List<Invoice_vwRegister> Headers { get; set; } = [];
        public List<Invoice_vwRegisterDetail> Details { get; set; } = [];
        public List<Invoice_vwRegisterCashCode> CashCodes { get; set; } = [];

        public Invoice_vwRegister? SelectedHeader { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        public decimal TotalInvoiceValue { get; set; }
        public decimal TotalTaxValue { get; set; }
        public decimal TotalGrossValue { get; set; }
        public decimal TotalDetailQuantity { get; set; }
        public decimal TotalCashCodeValue { get; set; }
    }
}
