using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace TradeControl.Web.Data
{
    public class Settings
    {
        NodeContext _context;

        public Settings(NodeContext context)
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
                    return (_context.App_tbOptions.Count() == 0 || _context.Usr_tbUsers.Count() == 0);
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
                if (_context.App_ActivePeriods.Count() == 0)
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
    }
}
