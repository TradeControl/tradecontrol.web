using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TradeControl.Web.Data
{
    public class Docs
    {
        NodeContext _context;

        public Docs(NodeContext context)
        {
            _context = context;
        }

        public async Task<bool> SetToPrinted(NodeEnum.DocType docType)
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("App.proc_DocDespool @p0", parameters: new[] { (short)docType });
                return result != 0;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<bool> DespoolAll()
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("App.proc_DocDespoolAll");
                return result != 0;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return false;
            }
        }
    }
}
