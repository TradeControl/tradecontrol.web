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

        public Task<string> CompanyAccountCode => Task.Run(() =>
        {
            try
            {
                return _context.App_tbOptions.First().AccountCode;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return string.Empty;
            }
        });

        public Task<string> SqlUserName => Task.Run(() =>
        {
            try
            {
                return _context.Usr_Credentials.First().LogonName;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return string.Empty;
            }
        });

        public Task<string> UserName => Task.Run(() =>
        {
            try
            {
                return _context.Usr_Credentials.First().UserName;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return string.Empty;
            }
        });

        public Task<string> UserId => Task.Run(() =>
        {
            try
            {
                return _context.Usr_Credentials.First().UserId;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return string.Empty;
            }
        });

        public async Task<string> CompanyName() => await _context.CompanyName;
    }
}
