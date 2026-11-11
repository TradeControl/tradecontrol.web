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

        public async Task<int?> EventLogDeleteThrough(string logCode)
        {
            try
            {
                DateTime cutoffOn = await _context.App_tbEventLogs
                    .Where(e => e.LogCode == logCode)
                    .Select(e => e.LoggedOn)
                    .SingleAsync();

                var cutoffParameter = new Microsoft.Data.SqlClient.SqlParameter("@CutoffOn", System.Data.SqlDbType.DateTime)
                {
                    Value = cutoffOn
                };
                var deletedCountParameter = new Microsoft.Data.SqlClient.SqlParameter("@DeletedCount", System.Data.SqlDbType.Int)
                {
                    Direction = System.Data.ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC App.proc_EventLogCleardown @CutoffOn, @DeletedCount OUTPUT",
                    cutoffParameter,
                    deletedCountParameter);

                return (int)deletedCountParameter.Value;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return null;
            }
        }
    }
}
