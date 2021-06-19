using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using System.IO;
using TradeControl.Web.Data;
using MimeKit.Utils;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace TradeControl.Web.Mail
{
    public class MailHeader
    {
        public MailSettings Settings { get; set; }
        public string Name { get; set; }
        public string EmailTo { get; set; }
        public string Subject { get; set; }
        public List<string> Attachments { get; set; } = new List<string>();
    }

    public class MailImage
    {
        public string Tag { get; set; }
        public string FileName { get; set; }
    }

    public class MailDocument : MailHeader
    {
        public string TemplateFileName { get; set; }
        public IDictionary<string, string> Arguments { get; set; } = new Dictionary<string, string>();
        public List<MailImage> Images { get; set; } = new List<MailImage>();
    }

    public class MailText : MailHeader
    {
        public string Body { get; set; }
        public bool IsHtml { get; set; } = false;        
    }

    public class MailSettings
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string HostName { get; set; }
        public int Port { get; set; }
    }

    public abstract class MailService
    {
        protected async Task SendDocument(MailDocument mailDocument)
        {
            BodyBuilder bodyBuilder = new();
            bodyBuilder.HtmlBody = await DocumentHtml(mailDocument);

            //e.g <img src="cid:[logo]" alt="logo">
            foreach (var image in mailDocument.Images)
            {
                var contentId = MimeUtils.GenerateMessageId();
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace($"[{image.Tag}]", contentId);
                var entity = bodyBuilder.LinkedResources.Add(image.FileName);
                entity.ContentId = contentId;
            }

            await Send(mailDocument.Settings, bodyBuilder, mailDocument);
        }        

        protected async Task<string> PreviewDocument(MailDocument mailDocument, string fileName = "")
        {
            string htmlBody = await DocumentHtml(mailDocument);

            foreach (var image in mailDocument.Images)
            {
                FileInfo fileInfo = new(image.FileName);
                htmlBody = htmlBody.Replace($"[{image.Tag}]", Path.Combine($"/{TemplateManager.ImagesSubFolder}", fileInfo.Name));
            }
            htmlBody = htmlBody.Replace("cid:", string.Empty);

            if (!string.IsNullOrEmpty(fileName))
            {
                using FileStream fs = File.Create(fileName);
                using TextWriter s = new StreamWriter(fs);
                s.Write(htmlBody);
            }

            return htmlBody;
        }

        protected async Task<string> DocumentHtml(MailDocument mailDocument)
        {
            string htmlBody = string.Empty;

            using (StreamReader streamReader = new(mailDocument.TemplateFileName))
            {
                htmlBody = await streamReader.ReadToEndAsync();
                streamReader.Close();
            }

            foreach (var argument in mailDocument.Arguments)
                htmlBody = htmlBody.Replace($"[{argument.Key}]", argument.Value);

            return htmlBody;
        }


        protected async Task SendText(MailText mailText)
        {
            BodyBuilder bodyBuilder = new();
            if (mailText.IsHtml)
                bodyBuilder.HtmlBody = mailText.Body;
            else
                bodyBuilder.TextBody = mailText.Body;

            await Send(mailText.Settings, bodyBuilder, mailText);
        }

        static async Task Send(MailSettings settings, BodyBuilder bodyBuilder, MailHeader mailHeader)
        {
            foreach (var attachment in mailHeader.Attachments)
            {
                if (attachment.Length > 0)
                {
                    FileInfo fileInfo = new(attachment);

                    var stream = File.OpenRead(fileInfo.FullName);

                    var formFile = new FormFile(stream, 0, stream.Length, fileInfo.Name, fileInfo.FullName)
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = new($"application/{fileInfo.Extension}")
                    };

                    using MemoryStream memoryStream = new();
                    formFile.CopyTo(memoryStream);

                    bodyBuilder.Attachments.Add(fileInfo.Name, memoryStream.ToArray(), ContentType.Parse(formFile.ContentType));
                }
            }

            MimeMessage message = new();

            message.Sender = MailboxAddress.Parse(settings.UserName);
            message.To.Add(new MailboxAddress(mailHeader.Name, mailHeader.EmailTo));
            message.Subject = mailHeader.Subject;
            message.Body = bodyBuilder.ToMessageBody();

            SmtpClient smtpClient = new();
            smtpClient.Connect(settings.HostName, settings.Port, SecureSocketOptions.Auto);
            smtpClient.Authenticate(settings.UserName, settings.Password);
            await smtpClient.SendAsync(message);
            smtpClient.Disconnect(true);

        }

        protected string GetDisplayName<T>(string propertyName)
        {
            MemberInfo property = typeof(T).GetProperty(propertyName);
            var attribute = property.GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().FirstOrDefault();
            return attribute?.Name;
        }
    }
}
