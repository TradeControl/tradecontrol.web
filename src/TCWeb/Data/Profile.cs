using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeControl.Web.Data
{
    public class Profile
    {
        NodeContext _context;

        public Profile(NodeContext context)
        {
            _context = context;
        }

        public async Task<string> CompanyAccountCode()
        {
            try
            {
                return await _context.App_tbOptions.Select(o => o.AccountCode).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<string> SqlUserName()
        {
            try
            {
                return await _context.Usr_Credentials.Select(u => u.LogonName).FirstAsync();
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<string> UserName(string aspnetId)
        {
            try
            {
                return await _context.GetUserName(aspnetId);
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<string> UserId(string aspnetId)
        {
            try
            {
                return await _context.GetUserId(aspnetId);
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<string> AspNetId(string aspnetId)
        {
            try
            {
                return await _context.GetAspNetId(aspnetId);
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<string> CompanyName() => await _context.CompanyName();

    }
}
