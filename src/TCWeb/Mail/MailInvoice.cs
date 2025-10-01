using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Mail
{
    public class MailInvoice : MailService
    {
        public string InvoiceNumber { get; }
        NodeContext NodeContext { get; }
        MailDocument Document { get; }

        public MailInvoice(NodeContext nodeContext, MailDocument mailDocument, string invoiceNumber) : base() 
        {
            NodeContext = nodeContext;
            InvoiceNumber = invoiceNumber;
            Document = mailDocument;
        }

        #region send/preview
        public async Task Send(string emailAddress)
        {
            try
            {
                string accountCode = await NodeContext.Invoice_tbInvoices
                                        .Where(i => i.InvoiceNumber == InvoiceNumber)
                                        .Select(i => i.AccountCode)
                                        .FirstAsync();

                var invoiceHeader = from c in NodeContext.Subject_EmailAddresses
                                    where c.AccountCode == accountCode && c.EmailAddress == emailAddress
                                    select new
                                    {
                                        Name = c.ContactName,
                                        EmailTo = c.EmailAddress
                                    };

                var emailInfo = await invoiceHeader.OrderBy(i => i.EmailTo).SingleOrDefaultAsync();

                if (emailInfo != null)
                {
                    Document.Name = emailInfo.Name;
                    Document.EmailTo = emailInfo.EmailTo;

                    await SendInvoice();

                    Invoices invoices = new (NodeContext, InvoiceNumber);
                    await invoices.SetToPrinted();

                }
                else
                    throw new Exception($"{emailAddress} is not registered to {accountCode}");
            }
            catch(Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        private async Task SendInvoice()
        {
            if (string.IsNullOrEmpty(Document.EmailTo))
                throw new Exception($"Request to send invoice {InvoiceNumber} has no email address");

            await BuildInvoice();
            await SendDocument(Document);
        }

        public async Task<string> PreviewInvoice(string fileName = "")
        {
            try
            {
                await BuildInvoice();
                return await PreviewDocument(Document, fileName);
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
        #endregion

        #region build invoice
        private async Task BuildInvoice()
        {
            await WriteCompanyDetails();
            await WriteInvoiceHeader();
            await WriteInvoiceDetails();
            await WriteTaxSummary();
        }

        private async Task WriteCompanyDetails()
        {
            var company = await NodeContext.Usr_Doc.Take(1).SingleOrDefaultAsync();            

            Document.Arguments.Add("CompanyName", company.CompanyName);
            Document.Arguments.Add("CompanyAddress", company.CompanyAddress);
            Document.Arguments.Add("CompanyEmailAddress", company.CompanyEmailAddress);
            Document.Arguments.Add("CompanyWebsite", company.CompanyWebsite);
            Document.Arguments.Add("CompanyPhoneNumber", company.CompanyPhoneNumber);
            Document.Arguments.Add("CompanyNumber", company.CompanyNumber);
            Document.Arguments.Add("VatNumber", company.VatNumber);
            Document.Arguments.Add("BankAccount", company.BankAccount);
            Document.Arguments.Add("BankSortCode", company.BankSortCode);
            Document.Arguments.Add("BankAccountNumber", company.BankAccountNumber);
        }

        private async Task WriteInvoiceHeader()
        {
            var invoice = await NodeContext.Invoice_Doc.Where(i => i.InvoiceNumber == InvoiceNumber).FirstAsync();

            Document.Subject = invoice.InvoiceType;
            Document.Arguments.Add("InvoiceType", invoice.InvoiceType);
            Document.Arguments.Add("AccountCode", invoice.AccountCode);
            Document.Arguments.Add("AccountName", invoice.AccountName);
            Document.Arguments.Add("EmailAddress", invoice.EmailAddress ?? string.Empty);
            Document.Arguments.Add("InvoiceNumber", invoice.InvoiceNumber);
            Document.Arguments.Add("UserName", invoice.UserName);
            Document.Arguments.Add("InvoicedOn", invoice.InvoicedOn.ToLongDateString());
            Document.Arguments.Add("DueOn", invoice.DueOn.ToLongDateString());
            Document.Arguments.Add("Notes", invoice.Notes ?? string.Empty);
            Document.Arguments.Add("PaymentTerms", invoice.PaymentTerms ?? string.Empty);
            Document.Arguments.Add("InvoiceAddress", invoice.InvoiceAddress ?? string.Empty);
            Document.Arguments.Add("InvoiceValue", invoice.InvoiceValue.ToString("C2"));
            Document.Arguments.Add("TaxValue", invoice.TaxValue.ToString("C2"));
            Document.Arguments.Add("TotalValue", invoice.TotalValue.ToString("C2"));
        }

        private async Task WriteInvoiceDetails()
        {

            StringBuilder invoiceItems = new ();
            invoiceItems.AppendLine(@"<table class=""DataGrid"">");
            invoiceItems.AppendLine(@"<thead class=""DataGridHeader""><tr>");

            invoiceItems.AppendLine($"<th>{GetDisplayName<Invoice_vwDocDetail>("ItemCode")}</th>");
            invoiceItems.AppendLine($"<th>{GetDisplayName<Invoice_vwDocDetail>("ItemDescription")}</th>");
            invoiceItems.AppendLine($"<th>{GetDisplayName<Invoice_vwDocDetail>("ItemReference")}</th>");
            invoiceItems.AppendLine($"<th>{GetDisplayName<Invoice_vwDocDetail>("TaxCode")}</th>");
            /* 
            invoiceItems.AppendLine($"<th>{GetDisplayName<Invoice_vwDocDetail>("ActionedOn")}</th>");
            invoiceItems.AppendLine($"<th>{GetDisplayName<Invoice_vwDocDetail>("Quantity")}</th>");
            invoiceItems.AppendLine($"<th>{GetDisplayName<Invoice_vwDocDetail>("UnitOfMeasure")}</th>");
            */
            invoiceItems.AppendLine(string.Concat(@"<th align=""right"">", $"{GetDisplayName<Invoice_vwDocDetail>("InvoiceValue")}</th>"));
            invoiceItems.AppendLine(string.Concat(@"<th align=""right"">", $"{GetDisplayName<Invoice_vwDocDetail>("TaxValue")}</th>"));
            invoiceItems.AppendLine(string.Concat(@"<th align=""right"">", $"{ GetDisplayName<Invoice_vwDocDetail>("TotalValue")}</th>"));

            invoiceItems.AppendLine("</tr></thead>");

            var details = await NodeContext.Invoice_DocDetails
                        .Where(i => i.InvoiceNumber == InvoiceNumber)
                        .OrderBy(i => i.ItemCode)
                        .ToListAsync();

            invoiceItems.AppendLine("<tbody>");

            foreach (var detail in details)
            {
                invoiceItems.AppendLine(@"<tr class=""DataGridItem"">");

                invoiceItems.AppendLine($"<td>{detail.ItemCode}</td>");
                invoiceItems.AppendLine($"<td>{detail.ItemDescription}</td>");
                invoiceItems.AppendLine($"<td>{detail.ItemReference}</td>");
                invoiceItems.AppendLine($"<td>{detail.TaxCode}</td>");
                /* 
                invoiceItems.AppendLine($"<td>{detail.ActionedOn.ToLongDateString()}</td>");
                invoiceItems.AppendLine($"<td>{detail.Quantity}</td>");
                invoiceItems.AppendLine($"<td>{detail.UnitOfMeasure}</td>");                                
                */
                invoiceItems.AppendLine(string.Concat(@"<th align=""right"">", $"{detail.InvoiceValue:C2}</td>"));
                invoiceItems.AppendLine(string.Concat(@"<th align=""right"">", $"{detail.TaxValue:C2}</td>"));
                invoiceItems.AppendLine(string.Concat(@"<th align=""right"">", $"{detail.TotalValue:C2}</td>"));

                invoiceItems.AppendLine("</tr>");
            }

            invoiceItems.AppendLine("</tbody>");
            invoiceItems.AppendLine("</table>");

            Document.Arguments.Add("InvoiceDetails", invoiceItems.ToString());
        }

        private async Task WriteTaxSummary()
        {

            StringBuilder taxItems = new();

            taxItems.AppendLine(@"<table class=""DataGrid"">");
            taxItems.AppendLine(@"<thead class=""DataGridHeader""><tr>");

            taxItems.AppendLine($"<th>{GetDisplayName<Invoice_vwTaxSummary>("TaxCode")}</th>");
            taxItems.AppendLine(string.Concat(@"<th align=""right"">", $"{GetDisplayName<Invoice_vwTaxSummary>("InvoiceValueTotal")}</th>"));
            taxItems.AppendLine(string.Concat(@"<th align=""right"">", $"{GetDisplayName<Invoice_vwTaxSummary>("TaxValueTotal")}</th>"));
            taxItems.AppendLine(string.Concat(@"<th align=""right"">", $"{GetDisplayName<Invoice_vwTaxSummary>("TaxRate")}</th>"));

            taxItems.AppendLine("</tr></thead>");

            var taxes = await NodeContext.Invoice_TaxSummary
                            .Where(i => i.InvoiceNumber == InvoiceNumber)
                            .OrderBy(i => i.TaxCode)
                            .ToListAsync();

            taxItems.AppendLine("<tbody>");

            foreach (var tax in taxes)
            {
                taxItems.AppendLine(@"<tr class=""DataGridItem"">");

                taxItems.AppendLine($"<td>{tax.TaxCode}</td>");
                taxItems.AppendLine(string.Concat(@"<th align=""right"">", $"{tax.InvoiceValueTotal:C2}</td>"));
                taxItems.AppendLine(string.Concat(@"<th align=""right"">", $"{tax.TaxValueTotal:C2}</td>"));
                taxItems.AppendLine(string.Concat(@"<th align=""right"">", $"{tax.TaxRate:P2}</td>"));

                taxItems.AppendLine("</tr>");
            }

            taxItems.AppendLine("</tbody>");
            taxItems.AppendLine("</table>");

            Document.Arguments.Add("TaxSummary", taxItems.ToString());
        }
        #endregion
    }

}
