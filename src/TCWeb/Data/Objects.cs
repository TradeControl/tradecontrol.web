using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Models;

namespace TradeControl.Web.Data
{
    public class Objects
    {
        readonly NodeContext _context;

        public string ObjectCode { get; } = string.Empty;

        public Objects(NodeContext context)
        {
            _context = context;
        }

        public Objects(NodeContext context, string activityCode)
        {
            _context = context;
            ObjectCode = activityCode;
        }

        public async Task<string> ParentObject() => await _context.ParentObject(ObjectCode);

        public IEnumerable<Object_proc_WorkFlow> ChildActivities(string parentObjectCode)
        {
            try
            {
                var _parentObjectCode = new SqlParameter()
                {
                    ParameterName = "@ParentObjectCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 50,
                    Value = parentObjectCode
                };

                var _activityCode = new SqlParameter()
                {
                    ParameterName = "@ObjectCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 50,
                    Value = ObjectCode
                };

                const string sql = "Object.proc_WorkFlow @ParentObjectCode, @ObjectCode";

                var results = _context.Object_WorkFlow.FromSqlRaw(sql, _parentObjectCode, _activityCode).ToList();

                return results.OrderBy(t => t.ObjectCode).Select(t => t);
            }
            catch (Exception e)
            {
                _ = _context.ErrorLog(e);
                return null;
            }
        }

        public async Task<short> NextStepNumber() => await _context.GetObjectStepNumber(ObjectCode);

        public async Task<short> NextAttributeOrder() => await _context.GetObjectAtttributeOrder(ObjectCode);

        public async Task<short> NextOperationNumber() => await _context.GetObjectOperationNumber(ObjectCode);

        public async Task<bool> MirrorAllocation(string accountCode, string allocationCode)
        {
            try
            {
                int result = await _context.Database.ExecuteSqlRawAsync("Object.proc_Mirror @p0, @p1, @p2", parameters: new[] { ObjectCode, accountCode, allocationCode });
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
