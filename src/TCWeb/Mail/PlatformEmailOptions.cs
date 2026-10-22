using System;
using System.Net.Mail;
using MailKit.Security;

namespace TradeControl.Web.Mail
{
    public enum PlatformEmailSecurity
    {
        StartTls,
        SslOnConnect
    }

    public sealed class PlatformEmailOptions
    {
        public const string SectionName = "PlatformEmail";

        public bool Enabled { get; set; }
        public string HostName { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SenderAddress { get; set; } = string.Empty;
        public string SenderName { get; set; } = "Trade Control";
        public PlatformEmailSecurity Security { get; set; } = PlatformEmailSecurity.StartTls;

        public bool IsValid() =>
            IsValidHostName(HostName)
            && Port is > 0 and <= 65535
            && !string.IsNullOrWhiteSpace(UserName)
            && !string.IsNullOrWhiteSpace(Password)
            && IsValidEmailAddress(SenderAddress)
            && !string.IsNullOrWhiteSpace(SenderName)
            && Enum.IsDefined(Security);

        public static bool IsValidHostName(string? value) =>
            !string.IsNullOrWhiteSpace(value)
            && !value.Contains("://", StringComparison.Ordinal)
            && Uri.CheckHostName(value) != UriHostNameType.Unknown;

        public static bool IsValidEmailAddress(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            try
            {
                return new MailAddress(value).Address == value;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }

    internal static class PlatformEmailSecurityExtensions
    {
        public static SecureSocketOptions ToSecureSocketOptions(this PlatformEmailSecurity security) =>
            security switch
            {
                PlatformEmailSecurity.StartTls => SecureSocketOptions.StartTls,
                PlatformEmailSecurity.SslOnConnect => SecureSocketOptions.SslOnConnect,
                _ => throw new ArgumentOutOfRangeException(nameof(security), security, null)
            };
    }
}
