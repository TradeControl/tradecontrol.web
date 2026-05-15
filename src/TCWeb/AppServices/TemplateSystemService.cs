using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;

namespace TradeControl.Web.AppServices
{
    public interface ITemplateSystemService
    {
        Task<TemplateSystemState> GetStateAsync();
        Task SaveAsync(TemplateSystemAssignments assignments);
    }

    public sealed class TemplateSystemService : ITemplateSystemService
    {
        private readonly NodeContext _nodeContext;
        private readonly SemaphoreSlim _dbGate = new(initialCount: 1, maxCount: 1);

        public TemplateSystemService(NodeContext nodeContext)
        {
            _nodeContext = nodeContext;
        }

        public async Task<TemplateSystemState> GetStateAsync()
        {
            await _dbGate.WaitAsync();
            try
            {
                var templateOptions = await _nodeContext.Web_tbTemplates
                    .AsNoTracking()
                    .OrderBy(item => item.TemplateFileName)
                    .Select(item => new TemplateSystemTemplateOption {
                        TemplateId = item.TemplateId,
                        TemplateFileName = item.TemplateFileName
                    })
                    .ToListAsync();

                var options = await _nodeContext.App_tbOptions
                    .AsNoTracking()
                    .OrderBy(item => item.Identifier)
                    .FirstOrDefaultAsync();

                return new TemplateSystemState {
                    TemplateOptions = templateOptions,
                    SupportRequestTemplateId = options?.SupportRequestTemplateId,
                    UserRegistrationTemplateId = options?.UserRegistrationTemplateId,
                    UserRegistrationConfirmTemplateId = options?.UserRegistrationConfirmTemplateId,
                    UserRegistrationAdminNotifyTemplateId = options?.UserRegistrationAdminNotifyTemplateId
                };
            }
            finally
            {
                _dbGate.Release();
            }
        }

        public async Task SaveAsync(TemplateSystemAssignments assignments)
        {
            await _dbGate.WaitAsync();
            try
            {
                var options = await _nodeContext.App_tbOptions
                    .OrderBy(item => item.Identifier)
                    .FirstOrDefaultAsync();

                if (options == null)
                    return;

                options.SupportRequestTemplateId = assignments.SupportRequestTemplateId;
                options.UserRegistrationTemplateId = assignments.UserRegistrationTemplateId;
                options.UserRegistrationConfirmTemplateId = assignments.UserRegistrationConfirmTemplateId;
                options.UserRegistrationAdminNotifyTemplateId = assignments.UserRegistrationAdminNotifyTemplateId;

                _nodeContext.Attach(options).State = EntityState.Modified;
                await _nodeContext.SaveChangesAsync();
            }
            finally
            {
                _dbGate.Release();
            }
        }
    }

    public sealed class TemplateSystemState
    {
        public IReadOnlyList<TemplateSystemTemplateOption> TemplateOptions { get; init; } = [];
        public int? SupportRequestTemplateId { get; init; }
        public int? UserRegistrationTemplateId { get; init; }
        public int? UserRegistrationConfirmTemplateId { get; init; }
        public int? UserRegistrationAdminNotifyTemplateId { get; init; }
    }

    public sealed class TemplateSystemTemplateOption
    {
        public int TemplateId { get; init; }
        public string TemplateFileName { get; init; } = string.Empty;
    }

    public sealed class TemplateSystemAssignments
    {
        public int? SupportRequestTemplateId { get; init; }
        public int? UserRegistrationTemplateId { get; init; }
        public int? UserRegistrationConfirmTemplateId { get; init; }
        public int? UserRegistrationAdminNotifyTemplateId { get; init; }
    }
}
