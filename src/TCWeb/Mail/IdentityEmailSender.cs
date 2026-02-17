using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.FileProviders;
using TradeControl.Web.Data;

namespace TradeControl.Web.Mail
{
    public sealed class IdentityEmailSender : IEmailSender
    {
        private readonly NodeContext _nodeContext;
        private readonly IFileProvider _fileProvider;

        public IdentityEmailSender(NodeContext nodeContext, IFileProvider fileProvider)
        {
            _nodeContext = nodeContext;
            _fileProvider = fileProvider;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                NodeSettings nodeSettings = new(_nodeContext);

                if (!nodeSettings.HasMailHost)
                    throw new Exception("Mail host needs configuring.");

                var settings = await nodeSettings.MailHost();
                if (settings == null)
                    throw new Exception("Mail host settings not available.");

                var sender = new IdentityMailSender();
                await sender.SendAsync(new MailText
                {
                    Settings = settings,
                    Name = email,
                    EmailTo = email,
                    Subject = subject,
                    Body = htmlMessage,
                    IsHtml = true
                });
            }
            catch (Exception e)
            {
                await _nodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task SendRegistrationConfirmAsync(string emailAddress, string confirmUrl)
        {
            try
            {
                TemplateManager templateManager = new(_nodeContext, _fileProvider);
                var doc = await templateManager.GetUserRegistrationConfirm();

                var companyName = await _nodeContext.CompanyName();

                doc.Name = emailAddress;
                doc.EmailTo = emailAddress;
                doc.Subject = "Confirm your email";

                doc.Arguments.Add("CompanyName", WebUtility.HtmlEncode(companyName ?? string.Empty));
                doc.Arguments.Add("EmailAddress", WebUtility.HtmlEncode(emailAddress));
                doc.Arguments.Add("ConfirmUrl", confirmUrl);

                var sender = new IdentityDocumentSender(_nodeContext, doc);
                await sender.SendAsync();
            }
            catch (Exception e)
            {
                await _nodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task SendRegistrationAdminNotifyAsync(IEnumerable<string> adminEmails, string registrantEmailAddress, string usersUrl)
        {
            try
            {
                var list = adminEmails
                    .Select(e => (e ?? string.Empty).Trim())
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (list.Count == 0)
                    return;

                TemplateManager templateManager = new(_nodeContext, _fileProvider);
                var doc = await templateManager.GetUserRegistrationAdminNotify();

                var companyName = await _nodeContext.CompanyName();

                foreach (var email in list)
                {
                    var perRecipient = CloneDocument(doc);

                    perRecipient.Name = email;
                    perRecipient.EmailTo = email;
                    perRecipient.Subject = "New registration request";

                    perRecipient.Arguments.Add("CompanyName", WebUtility.HtmlEncode(companyName ?? string.Empty));
                    perRecipient.Arguments.Add("EmailAddress", WebUtility.HtmlEncode(registrantEmailAddress));
                    perRecipient.Arguments.Add("UsersUrl", usersUrl);

                    var sender = new IdentityDocumentSender(_nodeContext, perRecipient);
                    await sender.SendAsync();
                }
            }
            catch (Exception e)
            {
                await _nodeContext.ErrorLog(e);
                throw;
            }
        }

        private static MailDocument CloneDocument(MailDocument source)
        {
            return new MailDocument
            {
                Settings = source.Settings,
                TemplateFileName = source.TemplateFileName
            };
        }

        private sealed class IdentityMailSender : MailService
        {
            public Task SendAsync(MailText mailText) => SendText(mailText);
        }

        private sealed class IdentityDocumentSender : MailService
        {
            private readonly NodeContext _nodeContext;
            private readonly MailDocument _document;

            public IdentityDocumentSender(NodeContext nodeContext, MailDocument document)
            {
                _nodeContext = nodeContext;
                _document = document;
            }

            public async Task SendAsync()
            {
                try
                {
                    await SendDocument(_document);
                }
                catch (Exception e)
                {
                    await _nodeContext.ErrorLog(e);
                    throw;
                }
            }
        }
    }
}
