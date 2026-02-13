using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Mail
{
    public class MailInvoice : MailService
    {
        public static IReadOnlySet<string> AllowedTemplateTags { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CompanyName",
            "CompanyAddress",
            "CompanyEmailAddress",
            "CompanyWebsite",
            "CompanyPhoneNumber",
            "CompanyNumber",
            "VatNumber",
            "BankAccount",
            "BankSortCode",
            "BankAccountNumber",

            "InvoiceType",
            "SubjectCode",
            "SubjectName",
            "EmailAddress",
            "InvoiceNumber",
            "UserName",
            "InvoicedOn",
            "DueOn",
            "Notes",
            "PaymentTerms",
            "InvoiceAddress",
            "InvoiceValue",
            "TaxValue",
            "TotalValue",

            "InvoiceDetailsHtml",
            "TaxSummaryHtml"
        };

        private const string EmbedDirectivePrefix = "Embed:";
        private const string DetailsEmbedName = "Details";
        private const string TaxEmbedName = "Tax";

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
                                        .Select(i => i.SubjectCode)
                                        .FirstAsync();

                var invoiceHeader = from c in NodeContext.Subject_EmailAddresses
                                    where c.SubjectCode == accountCode && c.EmailAddress == emailAddress
                                    select new {
                                        Name = c.ContactName,
                                        EmailTo = c.EmailAddress
                                    };

                var emailInfo = await invoiceHeader.OrderBy(i => i.EmailTo).SingleOrDefaultAsync();

                if (emailInfo != null)
                {
                    Document.Name = emailInfo.Name;
                    Document.EmailTo = emailInfo.EmailTo;

                    await SendInvoice();

                    Invoices invoices = new(NodeContext, InvoiceNumber);
                    await invoices.SetToPrinted();

                }
                else
                    throw new Exception($"{emailAddress} is not registered to {accountCode}");
            }
            catch (Exception e)
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
            await WriteInvoiceDetailsFromEmbeddedTemplateAsync();
            await WriteTaxSummaryFromEmbeddedTemplateAsync();
        }

        private static string StripEmbedDirectives(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            return Regex.Replace(
                html,
                @"\[\s*Embed:(?<name>[A-Za-z0-9_]+)\s*=\s*(?<key>[A-Za-z0-9_\-\/]+)\s*\]\s*",
                string.Empty,
                RegexOptions.IgnoreCase);
        }

        protected override async Task<string> DocumentHtml(MailDocument mailDocument)
        {
            string htmlBody;

            using (StreamReader streamReader = new(mailDocument.TemplateFileName))
            {
                htmlBody = await streamReader.ReadToEndAsync();
                streamReader.Close();
            }

            htmlBody = StripEmbedDirectives(htmlBody);

            foreach (var argument in mailDocument.Arguments)
                htmlBody = htmlBody.Replace($"[{argument.Key}]", argument.Value);

            return htmlBody;
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
            Document.Arguments.Add("SubjectCode", invoice.SubjectCode);
            Document.Arguments.Add("SubjectName", invoice.SubjectName);
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

        private async Task WriteInvoiceDetailsFromEmbeddedTemplateAsync()
        {
            var key = await GetEmbeddedTemplateKeyAsync(DetailsEmbedName);
            var embeddedHtml = await LoadEmbeddedTemplateHtmlAsync(key);

            var details = await NodeContext.Invoice_DocDetails
                .Where(i => i.InvoiceNumber == InvoiceNumber)
                .OrderBy(i => i.ItemCode)
                .ToListAsync();

            string rendered = RenderRepeatingBlockTemplate(
                templateKey: key,
                templateHtml: embeddedHtml,
                itemStartMarker: "<!--ITEM-->",
                itemEndMarker: "<!--/ITEM-->",
                applyShellTokens: shell => shell
                    .Replace("[Col_ItemCode]", GetDisplayName<Invoice_vwDocDetail>("ItemCode"))
                    .Replace("[Col_ItemDescription]", GetDisplayName<Invoice_vwDocDetail>("ItemDescription"))
                    .Replace("[Col_ItemReference]", GetDisplayName<Invoice_vwDocDetail>("ItemReference"))
                    .Replace("[Col_TaxCode]", GetDisplayName<Invoice_vwDocDetail>("TaxCode"))
                    .Replace("[Col_InvoiceValue]", GetDisplayName<Invoice_vwDocDetail>("InvoiceValue"))
                    .Replace("[Col_TaxValue]", GetDisplayName<Invoice_vwDocDetail>("TaxValue"))
                    .Replace("[Col_TotalValue]", GetDisplayName<Invoice_vwDocDetail>("TotalValue")),
                items: details,
                renderItem: (itemTemplate, d) => {
                    return itemTemplate
                        .Replace("[ItemCode]", d.ItemCode ?? string.Empty)
                        .Replace("[ItemDescription]", d.ItemDescription ?? string.Empty)
                        .Replace("[ItemReference]", d.ItemReference ?? string.Empty)
                        .Replace("[TaxCode]", d.TaxCode ?? string.Empty)
                        .Replace("[InvoiceValue]", d.InvoiceValue.ToString("C2"))
                        .Replace("[TaxValue]", d.TaxValue.ToString("C2"))
                        .Replace("[TotalValue]", d.TotalValue.ToString("C2"));
                }
            );

            Document.Arguments.Add("InvoiceDetailsHtml", rendered);
        }

        private async Task WriteTaxSummaryFromEmbeddedTemplateAsync()
        {
            var key = await GetEmbeddedTemplateKeyAsync(TaxEmbedName);
            var embeddedHtml = await LoadEmbeddedTemplateHtmlAsync(key);

            var taxes = await NodeContext.Invoice_TaxSummary
                .Where(i => i.InvoiceNumber == InvoiceNumber)
                .OrderBy(i => i.TaxCode)
                .ToListAsync();

            string rendered = RenderRepeatingBlockTemplate(
                templateKey: key,
                templateHtml: embeddedHtml,
                itemStartMarker: "<!--ITEM-->",
                itemEndMarker: "<!--/ITEM-->",
                applyShellTokens: shell => shell
                    .Replace("[Col_TaxCode]", GetDisplayName<Invoice_vwTaxSummary>("TaxCode"))
                    .Replace("[Col_InvoiceValueTotal]", GetDisplayName<Invoice_vwTaxSummary>("InvoiceValueTotal"))
                    .Replace("[Col_TaxValueTotal]", GetDisplayName<Invoice_vwTaxSummary>("TaxValueTotal"))
                    .Replace("[Col_TaxRate]", GetDisplayName<Invoice_vwTaxSummary>("TaxRate")),
                items: taxes,
                renderItem: (itemTemplate, t) => {
                    return itemTemplate
                        .Replace("[TaxCode]", t.TaxCode ?? string.Empty)
                        .Replace("[InvoiceValueTotal]", t.InvoiceValueTotal.ToString("C2"))
                        .Replace("[TaxValueTotal]", t.TaxValueTotal.ToString("C2"))
                        .Replace("[TaxRate]", t.TaxRate.ToString("P2"));
                }
            );

            Document.Arguments.Add("TaxSummaryHtml", rendered);
        }

        private async Task<string> GetEmbeddedTemplateKeyAsync(string embedName)
        {
            if (string.IsNullOrWhiteSpace(Document.TemplateFileName) || !File.Exists(Document.TemplateFileName))
                throw new Exception("Invoice template file not found.");

            string html;
            using (StreamReader sr = new(Document.TemplateFileName))
                html = await sr.ReadToEndAsync();

            var tagName = $"{EmbedDirectivePrefix}{embedName}";
            var key = ExtractDirectiveValue(html, tagName);
            if (string.IsNullOrWhiteSpace(key))
                throw new Exception($"Missing embedded template directive: [{tagName}=...]");

            return key;
        }

        private static string ExtractDirectiveValue(string html, string directiveName)
        {
            if (string.IsNullOrWhiteSpace(html) || string.IsNullOrWhiteSpace(directiveName))
                return string.Empty;

            var matches = Regex.Match(
                html,
                $@"\[\s*{Regex.Escape(directiveName)}\s*=\s*(?<value>[A-Za-z0-9_\-\/]+)\s*\]",
                RegexOptions.IgnoreCase);

            var value = matches.Success ? matches.Groups["value"]?.Value?.Trim() : string.Empty;
            return value ?? string.Empty;
        }

        private async Task<string> LoadEmbeddedTemplateHtmlAsync(string templateKey)
        {
            var embeddedFileName = $"{templateKey}.tpl";

            var dir = Path.GetDirectoryName(Document.TemplateFileName);
            if (string.IsNullOrWhiteSpace(dir))
                throw new Exception("Invoice template folder could not be resolved.");

            var filePath = Path.Combine(dir, embeddedFileName);
            if (!File.Exists(filePath))
                throw new Exception($"Embedded template not found: {embeddedFileName}");

            using StreamReader sr = new(filePath);
            return await sr.ReadToEndAsync();
        }

        private static string RenderRepeatingBlockTemplate<T>(
            string templateKey,
            string templateHtml,
            string itemStartMarker,
            string itemEndMarker,
            Func<string, string> applyShellTokens,
            IReadOnlyList<T> items,
            Func<string, T, string> renderItem)
        {
            int startIndex = templateHtml.IndexOf(itemStartMarker, StringComparison.OrdinalIgnoreCase);
            int endIndex = templateHtml.IndexOf(itemEndMarker, StringComparison.OrdinalIgnoreCase);

            if (startIndex < 0 || endIndex < 0 || endIndex <= startIndex)
                throw new Exception($"{templateKey}.tpl must contain a single repeating block delimited by {itemStartMarker} and {itemEndMarker}");

            int itemTemplateStart = startIndex + itemStartMarker.Length;
            string itemTemplate = templateHtml.Substring(itemTemplateStart, endIndex - itemTemplateStart);

            string shell = templateHtml.Remove(startIndex, (endIndex + itemEndMarker.Length) - startIndex);
            shell = applyShellTokens(shell);

            StringBuilder renderedItems = new();
            foreach (var item in items)
            {
                renderedItems.Append(renderItem(itemTemplate, item));
            }

            return shell.Replace("[Items]", renderedItems.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        public sealed class InvoiceTemplateParseProfile : TemplateManager.ITemplateParseProfile
        {
            public IReadOnlySet<string> AllowedFieldTags => AllowedTemplateTags;

            public IReadOnlySet<string> RequiredEmbeds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Details",
                "Tax"
            };

            public IReadOnlySet<string> RequiredOutputTags { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "InvoiceDetailsHtml",
                "TaxSummaryHtml"
            };
        }

        public static TemplateManager.ITemplateParseProfile ParseProfile { get; } = new InvoiceTemplateParseProfile();
    }
}
