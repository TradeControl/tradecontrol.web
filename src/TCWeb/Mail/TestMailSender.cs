using System.Threading.Tasks;

namespace TradeControl.Web.Mail
{
    internal sealed class TestMailSender : MailService
    {
        public TestMailSender(IPlatformEmailSender platformEmailSender) : base(platformEmailSender)
        {
        }

        public Task SendAsync(MailText mailText) => SendText(mailText);
    }
}
