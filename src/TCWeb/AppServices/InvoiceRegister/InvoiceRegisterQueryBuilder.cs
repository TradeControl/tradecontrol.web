using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeControl.Web.Data;
using TradeControl.Web.Models;
using TradeControl.Web.Pages.Invoice.Register.Models;

namespace TradeControl.Web.AppServices.InvoiceRegister
{
    public class InvoiceRegisterQueryBuilder : IInvoiceRegisterQueryBuilder
    {
        public IQueryable<Invoice_vwRegister> BuildHeaderQuery(NodeContext nodeContext, InvoiceFilterModel filter)
        {
            var query = nodeContext.Invoice_Register.AsQueryable();

            query = ApplyHeaderFilters(nodeContext, query, filter);

            return query;
        }

        public IQueryable<Invoice_vwRegisterDetail> BuildDetailQuery(NodeContext nodeContext, InvoiceFilterModel filter)
        {
            var query = nodeContext.Invoice_RegisterDetails.AsQueryable();

            query = ApplyDetailFilters(nodeContext, query, filter);

            return query;
        }

        public IQueryable<Invoice_vwRegisterCashCode> BuildCashCodeQuery(NodeContext nodeContext, InvoiceFilterModel filter)
        {
            var query = nodeContext.Invoice_RegisterCashCodes.AsQueryable();

            query = ApplyCashCodeFilters(query, filter);

            return query;
        }

        public IQueryable<Invoice_vwChangeLog> BuildChangeLogQuery(NodeContext nodeContext, InvoiceFilterModel filter)
        {
            var query = nodeContext.Invoice_ChangeLog.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SelectedInvoiceNumber))
            {
                query = query.Where(x => x.InvoiceNumber == filter.SelectedInvoiceNumber);
            }
            else
            {
                query = query.Where(x => false);
            }

            return query;
        }

        public IQueryable<Invoice_vwRegister> ApplyHeaderSorting(IQueryable<Invoice_vwRegister> query, InvoiceFilterModel filter)
        {
            var sortField = filter.SortField?.Trim();
            var descending = string.Equals(filter.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(sortField))
                return query.OrderByDescending(x => x.InvoicedOn).ThenByDescending(x => x.InvoiceNumber);

            return sortField.ToLowerInvariant() switch
            {
                "invoicenumber" => descending
                    ? query.OrderByDescending(x => x.InvoiceNumber)
                    : query.OrderBy(x => x.InvoiceNumber),

                "subjectname" => descending
                    ? query.OrderByDescending(x => x.SubjectName).ThenByDescending(x => x.InvoiceNumber)
                    : query.OrderBy(x => x.SubjectName).ThenBy(x => x.InvoiceNumber),

                "invoicevalue" => descending
                    ? query.OrderByDescending(x => x.InvoiceValue).ThenByDescending(x => x.InvoiceNumber)
                    : query.OrderBy(x => x.InvoiceValue).ThenBy(x => x.InvoiceNumber),

                "taxvalue" => descending
                    ? query.OrderByDescending(x => x.TaxValue).ThenByDescending(x => x.InvoiceNumber)
                    : query.OrderBy(x => x.TaxValue).ThenBy(x => x.InvoiceNumber),

                "totalinvoicevalue" => descending
                    ? query.OrderByDescending(x => x.TotalInvoiceValue).ThenByDescending(x => x.InvoiceNumber)
                    : query.OrderBy(x => x.TotalInvoiceValue).ThenBy(x => x.InvoiceNumber),

                "invoicestatuscode" => descending
                    ? query.OrderByDescending(x => x.InvoiceStatusCode).ThenByDescending(x => x.InvoiceNumber)
                    : query.OrderBy(x => x.InvoiceStatusCode).ThenBy(x => x.InvoiceNumber),

                "invoicetypecode" => descending
                    ? query.OrderByDescending(x => x.InvoiceTypeCode).ThenByDescending(x => x.InvoiceNumber)
                    : query.OrderBy(x => x.InvoiceTypeCode).ThenBy(x => x.InvoiceNumber),

                _ => descending
                    ? query.OrderByDescending(x => x.InvoicedOn).ThenByDescending(x => x.InvoiceNumber)
                    : query.OrderBy(x => x.InvoicedOn).ThenBy(x => x.InvoiceNumber)
            };
        }

        public Task<List<Invoice_vwRegister>> ApplyFormattingAsync(IEnumerable<Invoice_vwRegister> headers, IInvoiceFormattingService formattingService)
        {
            var items = headers.ToList();

            foreach (var header in items)
                formattingService.Apply(header);

            return Task.FromResult(items);
        }

        private static IQueryable<Invoice_vwRegister> ApplyHeaderFilters(
            NodeContext nodeContext,
            IQueryable<Invoice_vwRegister> query,
            InvoiceFilterModel filter)
        {
            query = ApplyPeriodFilter(query, filter);

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.InvoicedOn >= filter.DateFrom.Value.Date);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.InvoicedOn <= filter.DateTo.Value.Date);

            if (!string.IsNullOrWhiteSpace(filter.InvoiceType)
                && short.TryParse(filter.InvoiceType, out var invoiceTypeCode))
            {
                query = query.Where(x => x.InvoiceTypeCode == invoiceTypeCode);
            }

            if (!string.IsNullOrWhiteSpace(filter.InvoiceStatus)
                && short.TryParse(filter.InvoiceStatus, out var invoiceStatusCode))
            {
                query = query.Where(x => x.InvoiceStatusCode == invoiceStatusCode);
            }

            if (!string.IsNullOrWhiteSpace(filter.Namespace))
            {
                query = ApplyNamespaceFilter(query, filter.Namespace);
            }

            if (!string.IsNullOrWhiteSpace(filter.CashCode))
            {
                var invoiceNumbers = nodeContext.Invoice_RegisterDetails
                    .Where(x => x.CashCode == filter.CashCode)
                    .Select(x => x.InvoiceNumber)
                    .Distinct();

                query = query.Where(x => invoiceNumbers.Contains(x.InvoiceNumber));
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var search = filter.SearchText.Trim();
                query = query.Where(x =>
                    (x.InvoiceNumber != null && x.InvoiceNumber.Contains(search)) ||
                    (x.SubjectName != null && x.SubjectName.Contains(search)));
            }

            if (filter.StatusDraft)
                query = query.Where(x => x.InvoiceStatusCode == (short)NodeEnum.InvoiceStatus.Pending);

            if (filter.StatusPosted)
                query = query.Where(x => x.InvoiceStatusCode != (short)NodeEnum.InvoiceStatus.Pending);

            if (filter.StatusUnsent)
                query = query.Where(x => !x.Printed);

            if (filter.StatusUnpaid)
                query = query.Where(x => x.InvoiceStatusCode == (short)NodeEnum.InvoiceStatus.Invoiced
                    || x.InvoiceStatusCode == (short)NodeEnum.InvoiceStatus.PartiallyPaid);

            return query;
        }

        private static IQueryable<Invoice_vwRegisterDetail> ApplyDetailFilters(
            NodeContext nodeContext,
            IQueryable<Invoice_vwRegisterDetail> query,
            InvoiceFilterModel filter)
        {
            query = ApplyPeriodFilter(query, filter);

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.InvoicedOn >= filter.DateFrom.Value.Date);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.InvoicedOn <= filter.DateTo.Value.Date);

            if (!string.IsNullOrWhiteSpace(filter.InvoiceType)
                && short.TryParse(filter.InvoiceType, out var invoiceTypeCode))
            {
                query = query.Where(x => x.InvoiceTypeCode == invoiceTypeCode);
            }

            if (!string.IsNullOrWhiteSpace(filter.InvoiceStatus)
                && short.TryParse(filter.InvoiceStatus, out var invoiceStatusCode))
            {
                query = query.Where(x => x.InvoiceStatusCode == invoiceStatusCode);
            }

            if (!string.IsNullOrWhiteSpace(filter.Namespace))
            {
                query = ApplyNamespaceFilter(query, filter.Namespace);
            }

            if (!string.IsNullOrWhiteSpace(filter.CashCode))
                query = query.Where(x => x.CashCode == filter.CashCode);

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var search = filter.SearchText.Trim();
                query = query.Where(x =>
                    (x.InvoiceNumber != null && x.InvoiceNumber.Contains(search)) ||
                    (x.SubjectName != null && x.SubjectName.Contains(search)) ||
                    (x.CashDescription != null && x.CashDescription.Contains(search)) ||
                    (x.ItemReference != null && x.ItemReference.Contains(search)));
            }

            if (filter.StatusDraft)
                query = query.Where(x => x.InvoiceStatusCode == (short)NodeEnum.InvoiceStatus.Pending);

            if (filter.StatusPosted)
                query = query.Where(x => x.InvoiceStatusCode != (short)NodeEnum.InvoiceStatus.Pending);

            if (filter.StatusUnsent)
                query = query.Where(x => !x.Printed);

            if (filter.StatusUnpaid)
                query = query.Where(x => x.InvoiceStatusCode == (short)NodeEnum.InvoiceStatus.Invoiced
                    || x.InvoiceStatusCode == (short)NodeEnum.InvoiceStatus.PartiallyPaid);

            if (!string.IsNullOrWhiteSpace(filter.SelectedInvoiceNumber))
                query = query.Where(x => x.InvoiceNumber == filter.SelectedInvoiceNumber);

            return query;
        }

        private static IQueryable<Invoice_vwRegisterCashCode> ApplyCashCodeFilters(
            IQueryable<Invoice_vwRegisterCashCode> query,
            InvoiceFilterModel filter)
        {
            query = ApplyPeriodFilter(query, filter);

            if (!string.IsNullOrWhiteSpace(filter.CashCode))
                query = query.Where(x => x.CashCode == filter.CashCode);

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var search = filter.SearchText.Trim();
                query = query.Where(x =>
                    (x.PeriodName != null && x.PeriodName.Contains(search)) ||
                    (x.CashCode != null && x.CashCode.Contains(search)) ||
                    (x.CashDescription != null && x.CashDescription.Contains(search)) ||
                    (x.CashPolarity != null && x.CashPolarity.Contains(search)));
            }

            return query;
        }

        private static IQueryable<T> ApplyNamespaceFilter<T>(IQueryable<T> query, string namespaceFilter) where T : class
        {
            var segments = GetNamespaceSegments(namespaceFilter);

            if (segments.Length == 0)
                return query;

            var subjectCode = segments[^1];
            var parentSubjectCode = segments.Length > 1 ? segments[^2] : string.Empty;

            if (typeof(T) == typeof(Invoice_vwRegister))
            {
                var typedQuery = (IQueryable<Invoice_vwRegister>)query;

                typedQuery = typedQuery.Where(x => x.SubjectCode == subjectCode);

                if (!string.IsNullOrWhiteSpace(parentSubjectCode))
                {
                    typedQuery = typedQuery.Where(x => x.ParentSubjectCode == parentSubjectCode);
                }

                return (IQueryable<T>)typedQuery;
            }

            if (typeof(T) == typeof(Invoice_vwRegisterDetail))
            {
                var typedQuery = (IQueryable<Invoice_vwRegisterDetail>)query;

                typedQuery = typedQuery.Where(x => x.SubjectCode == subjectCode);

                if (!string.IsNullOrWhiteSpace(parentSubjectCode))
                {
                    typedQuery = typedQuery.Where(x => x.ParentSubjectCode == parentSubjectCode);
                }

                return (IQueryable<T>)typedQuery;
            }

            return query;
        }

        private static string[] GetNamespaceSegments(string namespaceFilter)
        {
            var normalisedFilter = namespaceFilter?.Trim().Trim('.') ?? string.Empty;

            return string.IsNullOrWhiteSpace(normalisedFilter)
                ? Array.Empty<string>()
                : normalisedFilter.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        private static IQueryable<T> ApplyPeriodFilter<T>(IQueryable<T> query, InvoiceFilterModel filter) where T : class
        {
            if (filter.ShowAll)
                return query;

            if (typeof(T) == typeof(Invoice_vwRegister))
            {
                var typedQuery = (IQueryable<Invoice_vwRegister>)query;

                if (filter.PeriodYear.HasValue && filter.PeriodYear.Value > 0)
                {
                    typedQuery = typedQuery.Where(x => x.StartOn.Year == filter.PeriodYear.Value);

                    if (filter.PeriodMonth.HasValue && filter.PeriodMonth.Value > 0)
                        typedQuery = typedQuery.Where(x => x.StartOn.Month == filter.PeriodMonth.Value);
                }
                else if (filter.DateFrom.HasValue)
                {
                    typedQuery = typedQuery.Where(x => x.StartOn >= filter.DateFrom.Value.Date);
                }

                return (IQueryable<T>)typedQuery;
            }

            if (typeof(T) == typeof(Invoice_vwRegisterDetail))
            {
                var typedQuery = (IQueryable<Invoice_vwRegisterDetail>)query;

                if (filter.PeriodYear.HasValue && filter.PeriodYear.Value > 0)
                {
                    typedQuery = typedQuery.Where(x => x.StartOn.Year == filter.PeriodYear.Value);

                    if (filter.PeriodMonth.HasValue && filter.PeriodMonth.Value > 0)
                        typedQuery = typedQuery.Where(x => x.StartOn.Month == filter.PeriodMonth.Value);
                }
                else if (filter.DateFrom.HasValue)
                {
                    typedQuery = typedQuery.Where(x => x.StartOn >= filter.DateFrom.Value.Date);
                }

                return (IQueryable<T>)typedQuery;
            }

            if (typeof(T) == typeof(Invoice_vwRegisterCashCode))
            {
                var typedQuery = (IQueryable<Invoice_vwRegisterCashCode>)query;

                if (filter.PeriodYear.HasValue && filter.PeriodYear.Value > 0)
                {
                    typedQuery = typedQuery.Where(x => x.StartOn.Year == filter.PeriodYear.Value);

                    if (filter.PeriodMonth.HasValue && filter.PeriodMonth.Value > 0)
                        typedQuery = typedQuery.Where(x => x.StartOn.Month == filter.PeriodMonth.Value);
                }
                else if (filter.DateFrom.HasValue)
                {
                    typedQuery = typedQuery.Where(x => x.StartOn >= filter.DateFrom.Value.Date);
                }

                return (IQueryable<T>)typedQuery;
            }

            return query;
        }
    }
}
