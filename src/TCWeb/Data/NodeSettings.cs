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
                    _context.ErrorLog(e);
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
                    _context.ErrorLog(e);
                }
            }
        }

        public bool IsFirstUse
        {
            get
            {
                try
                { 
                    return (!_context.App_tbOptions.Any() || !_context.Usr_tbUsers.Any());
                }
                catch (Exception e)
                {
                    _context.ErrorLog(e);
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
                    _context.ErrorLog(e);
                    return false;
                }
            }
        }
        #endregion

        public Task<double> DataVersion => Task.Run(() =>
        {
            try
            {
                return _context.App_Version.First().SqldataVersion;
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
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
                _context.ErrorLog(e);
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
                _context.ErrorLog(e);
                return NodeEnum.CoinType.Fiat;
            }

        });

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
                _context.ErrorLog(e);
                return false;
            }
        }

        public async Task<MailSettings> MailHost()
        {
            try
            {
                var defaultHost = await _context.App_Host.OrderBy(h => h.HostId).SingleOrDefaultAsync();

                if (defaultHost == null)
                    return null;
                else
                    return new()
                    {
                        HostName = defaultHost.HostName,
                        UserName =  defaultHost.EmailAddress,
                        Password = defaultHost.EmailPassword,
                        Port = defaultHost.HostPort
                    };
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return null;
            }
        }
    }
}
