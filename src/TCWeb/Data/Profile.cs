using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeControl.Web.Model;

namespace TradeControl.Web.Data
{
    public class Profile
    {
        NodeContext _context;

        public Profile(NodeContext context)
        {
            _context = context;
        }

        public async Task<string> CompanySubjectCode()
        {
            try
            {
                return await _context.App_tbOptions.Select(o => o.SubjectCode).FirstOrDefaultAsync();
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

        /// <summary>
        /// Retrieves the configured Theme Code for a given user context. 
        /// Falls back safely to 'default' if no preference has been configured.
        /// </summary>
        public async Task<string> GetUserThemeCode(string aspnetId)
        {
            try
            {
                string internalUserId = await _context.GetUserId(aspnetId);

                if (string.IsNullOrEmpty(internalUserId))
                    return "default";

                string? themeCode = await _context.Usr_tbUsers
                    .Where(u => u.UserId == internalUserId && u.IsEnabled == 1)
                    .Select(u => u.ThemeCode)
                    .FirstOrDefaultAsync();

                return themeCode ?? "default";
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return "default";
            }
        }

        /// <summary>
        /// Persists a new theme setting for the user context into Usr.tbUser.
        /// </summary>
        public async Task<bool> UpdateUserThemeCode(string aspnetId, string newThemeCode)
        {
            try
            {
                string internalUserId = await _context.GetUserId(aspnetId);

                if (string.IsNullOrEmpty(internalUserId))
                    return false;

                var userRecord = await _context.Usr_tbUsers
                    .FirstOrDefaultAsync(u => u.UserId == internalUserId);

                if (userRecord == null)
                    return false;

                userRecord.ThemeCode = newThemeCode;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<List<Usr_tbTheme>> GetThemes()
        {
            try
            {
                return await _context.Set<Usr_tbTheme>()
                    .Where(t => t.IsEnabled)
                    .OrderBy(t => t.ThemeName)
                    .ToListAsync();
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return new List<Usr_tbTheme>();
            }
        }

        public async Task<string> GetUserThemeCssFile(string aspnetId)
        {
            try
            {
                string themeCode = await GetUserThemeCode(aspnetId);

                if (string.IsNullOrWhiteSpace(themeCode) || themeCode == "default")
                    return "theme-blue.css";

                string? cssFile = await _context.Set<Usr_tbTheme>()
                    .Where(t => t.ThemeCode == themeCode && t.IsEnabled)
                    .Select(t => t.CssFile)
                    .FirstOrDefaultAsync();

                return string.IsNullOrWhiteSpace(cssFile) ? "theme-blue.css" : cssFile;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return "theme-blue.css";
            }
        }
    }
}
