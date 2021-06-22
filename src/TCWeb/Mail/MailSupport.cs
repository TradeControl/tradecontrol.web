using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Mail
{
    public class MailSupport : MailService
    {
        string LogCode { get; }
        MailDocument Document { get; }
        NodeContext NodeContext { get; }
      
        public MailSupport(NodeContext nodeContext, MailDocument mailDocument, string logCode) : base()
        {
            NodeContext = nodeContext;
            LogCode = logCode;
            Document = mailDocument;
        }
        public async Task Send(string emailAddress)
        {
            await Send(emailAddress, string.Empty);
        }

        public async Task Send(string emailAddress, string note)
        {
            try
            {
                Document.Name = emailAddress;
                Document.EmailTo = emailAddress;
                Document.Subject = "support";

                await WriteCompanyDetails();

                var log = await NodeContext.App_tbEventLogs.FirstAsync(l => l.LogCode == LogCode);
                Document.Arguments.Add("LogCode", log.LogCode);
                Document.Arguments.Add("EventMessage", WebUtility.HtmlEncode(log.EventMessage));
                Document.Arguments.Add("Note", WebUtility.HtmlEncode(note));

                await SendDocument(Document);
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
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
        }

    }
}
