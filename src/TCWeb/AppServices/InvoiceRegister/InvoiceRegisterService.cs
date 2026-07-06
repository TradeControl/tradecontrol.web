using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeControl.Web.Data;
using TradeControl.Web.Models;
using TradeControl.Web.Pages.Invoice.Register.Models;

namespace TradeControl.Web.AppServices.InvoiceRegister
{
    public class InvoiceRegisterService : IInvoiceRegisterService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IInvoiceRegisterQueryBuilder _queryBuilder;
        private readonly IInvoiceFormattingService _formattingService;

        public InvoiceRegisterService(
            IServiceScopeFactory scopeFactory,
            IInvoiceRegisterQueryBuilder queryBuilder,
            IInvoiceFormattingService formattingService)
        {
            _scopeFactory = scopeFactory;
            _queryBuilder = queryBuilder;
            _formattingService = formattingService;
        }

        public async Task<InvoiceRegisterResult> QueryAsync(InvoiceFilterModel filter)
        {
            filter ??= new InvoiceFilterModel();

            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var headerQuery = _queryBuilder.BuildHeaderQuery(nodeContext, filter);
            var detailQuery = _queryBuilder.BuildDetailQuery(nodeContext, filter);
            var cashCodeQuery = _queryBuilder.BuildCashCodeQuery(nodeContext, filter);
            var changeLogQuery = _queryBuilder.BuildChangeLogQuery(nodeContext, filter);

            var totalItems = await headerQuery.CountAsync();

            var orderedHeaderQuery = _queryBuilder.ApplyHeaderSorting(headerQuery, filter);

            var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
            var pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
            var totalPages = totalItems == 0 ? 1 : (int)System.Math.Ceiling(totalItems / (double)pageSize);

            if (pageNumber > totalPages)
                pageNumber = totalPages;

            var headers = await orderedHeaderQuery.ToListAsync();
            headers = await _queryBuilder.ApplyFormattingAsync(headers, _formattingService);

            var details = await detailQuery
                .OrderBy(x => x.InvoiceNumber)
                .ThenBy(x => x.ProjectCode)
                .ThenBy(x => x.CashCode)
                .ToListAsync();

            var cashCodes = await cashCodeQuery
                .OrderBy(x => x.StartOn)
                .ThenBy(x => x.CashCode)
                .ToListAsync();

            var changeLog = await changeLogQuery
                .OrderByDescending(x => x.ChangedOn)
                .ThenByDescending(x => x.LogId)
                .ToListAsync();

            Invoice_vwRegister? selectedHeader = null;

            if (!string.IsNullOrWhiteSpace(filter.SelectedInvoiceNumber))
            {
                selectedHeader = await nodeContext.Invoice_Register
                    .Where(x => x.InvoiceNumber == filter.SelectedInvoiceNumber)
                    .FirstOrDefaultAsync();

                if (selectedHeader is not null)
                    _formattingService.Apply(selectedHeader);
            }

            var summaryHeaderQuery = _queryBuilder.BuildHeaderQuery(nodeContext, filter);
            var summaryDetailQuery = _queryBuilder.BuildDetailQuery(nodeContext, filter);
            var summaryCashCodeQuery = _queryBuilder.BuildCashCodeQuery(nodeContext, filter);

            var totalInvoiceValue = await summaryHeaderQuery.SumAsync(x => (decimal?)x.InvoiceValue) ?? 0m;
            var totalTaxValue = await summaryHeaderQuery.SumAsync(x => (decimal?)x.TaxValue) ?? 0m;
            var totalGrossValue = await summaryHeaderQuery.SumAsync(x => (decimal?)x.TotalInvoiceValue) ?? 0m;
            var totalDetailQuantity = await summaryDetailQuery.SumAsync(x => (decimal?)x.Quantity) ?? 0m;
            var totalCashCodeValue = await summaryCashCodeQuery.SumAsync(x => (decimal?)x.TotalValue) ?? 0m;

            return new InvoiceRegisterResult
            {
                Headers = headers,
                Details = details,
                CashCodes = cashCodes,
                ChangeLog = changeLog,
                SelectedHeader = selectedHeader,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                TotalInvoiceValue = totalInvoiceValue,
                TotalTaxValue = totalTaxValue,
                TotalGrossValue = totalGrossValue,
                TotalDetailQuantity = totalDetailQuantity,
                TotalCashCodeValue = totalCashCodeValue
            };
        }
    }
}
