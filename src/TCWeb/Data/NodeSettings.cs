using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Mail;

namespace TradeControl.Web.Data
{
    public class NodeSettings
    {
        readonly NodeContext _context;

        public NodeSettings(NodeContext context)
        {
            _context = context;
        }

        #region web install
        public bool IsInitialised
        {
            get
            {
                try
                {
                    return _context.App_tbOptions.First().IsInitialised;
                }
                catch (Exception e)
                {
                    _ = _context.ErrorLog(e);
                    return false;
                }
            }
            set
            {
                try
                {
                    int result = _context.Database.ExecuteSqlRaw("App.proc_Initialised @p0", parameters: new[] { value });
                }
                catch (Exception e)
                {
                    _ = _context.ErrorLog(e);
                }
            }
        }

        public bool IsFirstUse
        {
            get
            {
                try
                { 
                    return (!_context.AspNet_UserRegistrations.Any());
                }
                catch (Exception e)
                {
                    _ = _context.ErrorLog(e);
                    return false;
                }
            }
        }

        public bool HasMailHost
        {
            get
            {
                try
                {
                    return (_context.App_Host.Any());
                }
                catch (Exception e)
                {
                    _ = _context.ErrorLog(e);
                    return false;
                }
            }
        }

        public Task<double> DataVersion => Task.Run(() =>
        {
            try
            {
                return _context.App_Version.First().SqldataVersion;
            }
            catch (Exception e)
            {
                _ = _context.ErrorLog(e);
                return 0;
            }
        });

        public Task<bool> IsPeriodEnd => Task.Run(() =>
        {
            try
            {
                if (!_context.App_ActivePeriods.Any())
                    return false;
                else
                    return _context.App_ActivePeriods.First().EndOn < DateTime.Today;
            }
            catch (Exception e)
            {
                _ = _context.ErrorLog(e);
                return false;
            }
        });

        public Task<NodeEnum.CoinType> CoinType => Task.Run(() =>
        {
            try
            {
                return (NodeEnum.CoinType)_context.App_tbOptions.First().CoinTypeCode;
            }
            catch (Exception e)
            {
                _ = _context.ErrorLog(e);
                return NodeEnum.CoinType.Fiat;
            }

        });
        #endregion

        #region mail
        /// <summary>
        /// Modify key bytes to protect passwords in an unsecured Sql Server context
        /// </summary>
        public static byte[] SymmetricKey
        {
            get
            {
                byte[] key = { 0x22, 0x5C, 0x53, 0x4B, 0x44, 0x2D, 0x6B, 0x6D, 0x51, 0xC, 0x58, 0x69, 0x4C, 0x56, 0x72, 0x15 };
                return key;
            }
        }

        /// <summary>
        /// Modify vector bytes to protect passwords in an unsecured Sql Server context
        /// </summary>
        public static byte[] SymmetricVector
        {
            get
            {
                byte[] iv = { 0x5C, 0x6B, 0xF, 0x1A, 0x5A, 0x70, 0x74, 0x71, 0x2A, 0x79, 0x14, 0x56, 0x6A, 0x77, 0x9, 0x22 };
                return iv;
            }
        }

        public async Task<bool> SetHost(int? hostId)
        {
            try
            {
                var options = await _context.App_tbOptions.FirstAsync();
                options.HostId = hostId;
                _context.Attach(options).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.App_tbOptions.AnyAsync())
                        return false;
                }

                return true;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<MailSettings> MailHost()
        {
            try
            {
                Encrypt encrypt = new Encrypt(NodeSettings.SymmetricKey, NodeSettings.SymmetricVector);
                var defaultHost = await _context.App_Host.OrderBy(h => h.HostId).SingleOrDefaultAsync();

                if (defaultHost == null)
                    return null;
                else
                    return new()
                    {
                        HostName = defaultHost.HostName,
                        UserName =  defaultHost.EmailAddress,
                        Password = encrypt.DecryptString(defaultHost.EmailPassword),
                        Port = defaultHost.HostPort
                    };
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return null;
            }
        }
        #endregion

    }
}
