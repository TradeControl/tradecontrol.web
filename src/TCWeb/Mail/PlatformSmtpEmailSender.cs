using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace TradeControl.Web.Mail
{
    public interface IPlatformEmailSender
    {
        bool IsConfigured { get; }
        Task<string> SendAsync(MailHeader header, BodyBuilder bodyBuilder,
            CancellationToken cancellationToken = default);
    }

    public sealed class PlatformSmtpEmailSender : IPlatformEmailSender
    {
        private readonly PlatformEmailOptions _options;

        public PlatformSmtpEmailSender(IOptions<PlatformEmailOptions> options)
        {
            _options = options.Value;
            IsConfigured = _options.Enabled && _options.IsValid();
        }

        public bool IsConfigured { get; }

        public async Task<string> SendAsync(MailHeader header, BodyBuilder bodyBuilder,
            CancellationToken cancellationToken = default)
        {
            if (!IsConfigured)
                throw new InvalidOperationException("The platform email service is not configured.");

            MimeMessage message = new();
            var sender = new MailboxAddress(_options.SenderName, _options.SenderAddress);
            message.From.Add(sender);
            message.Sender = sender;
            message.To.Add(new MailboxAddress(header.Name, header.EmailTo));
            message.Subject = header.Subject ?? string.Empty;
            message.Body = bodyBuilder.ToMessageBody();

            if (PlatformEmailOptions.IsValidEmailAddress(header.ReplyTo))
                message.ReplyTo.Add(MailboxAddress.Parse(header.ReplyTo));

            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync(
                _options.HostName,
                _options.Port,
                _options.Security.ToSecureSocketOptions(),
                cancellationToken);
            await smtpClient.AuthenticateAsync(
                _options.UserName,
                _options.Password,
                cancellationToken);

            var response = await smtpClient.SendAsync(message, cancellationToken);
            await smtpClient.DisconnectAsync(true, cancellationToken);
            return response;
        }
    }
}
