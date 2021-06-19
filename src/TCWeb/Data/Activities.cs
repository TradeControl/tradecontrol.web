using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Models;

namespace TradeControl.Web.Data
{
    public class Activities
    {
        readonly NodeContext _context;

        public string ActivityCode { get; } = string.Empty;

        public Activities(NodeContext context)
        {
            _context = context;
        }

        public Activities(NodeContext context, string activityCode)
        {
            _context = context;
            ActivityCode = activityCode;
        }

        public async Task<string> ParentActivity() => await _context.ParentActivity(ActivityCode);
        

        public IEnumerable<Activity_proc_WorkFlow> ChildActivities(string parentActivityCode)
        {
            try
            {
                var _parentActivityCode = new SqlParameter()
                {
                    ParameterName = "@ParentActivityCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 50,
                    Value = parentActivityCode
                };

                var _activityCode = new SqlParameter()
                {
                    ParameterName = "@ActivityCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 50,
                    Value = ActivityCode
                };

                string sql = $"Activity.proc_WorkFlow @ParentActivityCode, @ActivityCode";

                var results = _context.Activity_WorkFlow.FromSqlRaw(sql, _parentActivityCode, _activityCode).ToList();

                return results.OrderBy(t => t.ActivityCode).Select(t => t);
            }
            catch (Exception e)
            {
                _context.ErrorLog(e);
                return null;
            }
        }

        public async Task<short> NextStepNumber() => await _context.GetActivityStepNumber(ActivityCode);

        public async Task<short> NextAttributeOrder() => await _context.GetActivityAtttributeOrder(ActivityCode);

        public async Task<short> NextOperationNumber() => await _context.GetActivityOperationNumber(ActivityCode);

        public async Task<bool> MirrorAllocation(string accountCode, string allocationCode)
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Activity.proc_Mirror @p0, @p1, @p2", parameters: new[] { ActivityCode, accountCode, allocationCode });
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
