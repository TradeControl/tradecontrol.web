using System.Threading.Tasks;

namespace TradeControl.Web.Mail
{
    internal sealed class TestMailSender : MailService
    {
        public Task SendAsync(MailText mailText) => SendText(mailText);
    }
}
