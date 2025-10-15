using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Admin.EventLog
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(NodeContext context) : base(context) { }

        public IList<App_vwEventLog> App_EventLog { get; set; }
        public IList<App_tbEventType> EventTypes { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public short? SelectedEventType { get; set; }

        public async Task OnGetAsync(int? pageNumber, int? pageSize, short? eventTypeFilter)
        {
            await SetViewData();

            PageNumber = pageNumber ?? 1;
            PageSize = pageSize ?? 10;
            SelectedEventType = eventTypeFilter;

            EventTypes = await NodeContext.App_tbEventTypes.OrderBy(t => t.EventType).ToListAsync();

            var query = NodeContext.App_EventLogs.AsQueryable();

            if (SelectedEventType.HasValue)
                query = query.Where(e => e.EventTypeCode == SelectedEventType.Value);

            int totalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            App_EventLog = await query
                .OrderByDescending(e => e.LoggedOn)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public static string TruncateMessage(string message, int maxWords = 25)
        {
            if (string.IsNullOrWhiteSpace(message)) return string.Empty;
            var words = message.Split(' ');
            if (words.Length <= maxWords) return message;
            return string.Join(' ', words.Take(maxWords)) + " ...";
        }
    }
}