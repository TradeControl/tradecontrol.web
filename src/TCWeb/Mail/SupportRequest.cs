using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using TradeControl.Web.Data;

namespace TradeControl.Web.Mail
{
    public class SupportRequest : MailService
    {
        MailText MailText { get; }
        NodeContext NodeContext { get; }

        public static string SupportAddress { get; } = "office@tradecontrol.co.uk";
        
        public SupportRequest(NodeContext nodeContext, MailText mailText) : base()
        {
            MailText = mailText;
            NodeContext = nodeContext;
        }

        public async Task Send()
        {
            try
            {
                await SendText(MailText);
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }

    }
}
