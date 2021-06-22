using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeControl.Web.Data
{
    public class NodeAdmin
    {
        NodeContext _context;
        public NodeAdmin(NodeContext context)
        {
            _context = context;
        }

        public async Task<bool> EventLogCleardown(string logCode)
        {
            try
            {
                
                DateTime loggedOn = await _context.App_tbEventLogs.Where(e => e.LogCode == logCode).Select(e => e.LoggedOn).FirstAsync();
                int retentionDays = (int)DateTime.Now.Subtract(loggedOn).TotalDays - 1;
                int result = await _context.Database.ExecuteSqlRawAsync("App.proc_EventLogCleardown @p0", retentionDays);

                return result != 0;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }
    }
}
