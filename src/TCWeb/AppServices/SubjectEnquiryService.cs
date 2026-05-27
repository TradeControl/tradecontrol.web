using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;

namespace TradeControl.Web.AppServices
{
    public sealed class SubjectEnquiryService : ISubjectEnquiryService
    {
        private readonly NodeContext _nodeContext;

        public SubjectEnquiryService(NodeContext nodeContext)
        {
            _nodeContext = nodeContext;
        }

        public async Task<SubjectEnquirySummary?> GetSummaryAsync(
            string subjectCode,
            string? parentSubjectCode = null,
            CancellationToken cancellationToken = default)
        {
            var normalizedSubjectCode = NormalizeCode(subjectCode);

            if (string.IsNullOrWhiteSpace(normalizedSubjectCode))
            {
                return null;
            }

            var resolvedParentSubjectCode = await ResolveParentSubjectCodeAsync(
                normalizedSubjectCode,
                parentSubjectCode,
                cancellationToken);

            var summary = await (
                from subject in _nodeContext.Subject_tbSubjects.AsNoTracking()
                join subjectType in _nodeContext.Subject_tbTypes.AsNoTracking()
                    on subject.SubjectTypeCode equals subjectType.SubjectTypeCode
                where subject.SubjectCode == normalizedSubjectCode
                select new {
                    subject.SubjectCode,
                    subject.SubjectName,
                    subjectType.SubjectType,
                    subjectType.CashPolarityCode
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (summary is null)
            {
                return null;
            }

            var currentBalance = await _nodeContext.Subject_Statement
                .AsNoTracking()
                .Where(o => o.SubjectCode == normalizedSubjectCode)
                .Where(o => resolvedParentSubjectCode == null
                    ? o.ParentSubjectCode == null
                    : o.ParentSubjectCode == resolvedParentSubjectCode)
                .OrderByDescending(o => o.RowNumber)
                .Select(o => (double?)o.Balance)
                .FirstOrDefaultAsync(cancellationToken) ?? 0d;

            return new SubjectEnquirySummary(
                summary.SubjectCode,
                summary.SubjectName,
                summary.SubjectType,
                (NodeEnum.CashPolarity)summary.CashPolarityCode,
                currentBalance,
                resolvedParentSubjectCode);
        }

        public async Task<SubjectEnquiryPageResult<SubjectEnquiryInvoiceItem>> GetInvoicesAsync(
            string subjectCode,
            int pageNumber,
            int pageSize,
            string? parentSubjectCode = null,
            CancellationToken cancellationToken = default)
        {
            var normalizedSubjectCode = NormalizeCode(subjectCode);
            var resolvedParentSubjectCode = await ResolveParentSubjectCodeAsync(
                normalizedSubjectCode,
                parentSubjectCode,
                cancellationToken);
            var (normalizedPageNumber, normalizedPageSize) = NormalizePaging(pageNumber, pageSize);

            var query =
                from invoice in _nodeContext.Invoice_tbInvoices.AsNoTracking()
                join invoiceType in _nodeContext.Invoice_tbTypes.AsNoTracking()
                    on invoice.InvoiceTypeCode equals invoiceType.InvoiceTypeCode
                join invoiceStatus in _nodeContext.Invoice_tbStatuses.AsNoTracking()
                    on invoice.InvoiceStatusCode equals invoiceStatus.InvoiceStatusCode
                where invoice.SubjectCode == normalizedSubjectCode
                   && (resolvedParentSubjectCode == null
                        ? invoice.ParentSubjectCode == null
                        : invoice.ParentSubjectCode == resolvedParentSubjectCode)
                orderby invoice.InvoicedOn descending, invoice.InvoiceNumber descending
                select new
                {
                    invoice.InvoiceNumber,
                    InvoiceType = invoiceType.InvoiceType,
                    invoice.InvoicedOn,
                    invoice.InvoiceValue,
                    invoice.TaxValue,
                    TotalPaidValue = invoice.PaidValue + invoice.PaidTaxValue,
                    InvoiceStatus = invoiceStatus.InvoiceStatus
                };

            var totalCount = await query.CountAsync(cancellationToken);

            var rows = await query
                .Skip((normalizedPageNumber - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .ToListAsync(cancellationToken);

            var items = rows
                .Select(o => new SubjectEnquiryInvoiceItem(
                    o.InvoiceNumber,
                    o.InvoiceType,
                    o.InvoicedOn,
                    Convert.ToDouble(o.InvoiceValue),
                    Convert.ToDouble(o.TaxValue),
                    Convert.ToDouble(o.TotalPaidValue),
                    o.InvoiceStatus,
                    $"/Invoice/Register/Index?InvoiceNumber={Uri.EscapeDataString(o.InvoiceNumber)}"))
                .ToList();

            return new SubjectEnquiryPageResult<SubjectEnquiryInvoiceItem>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = normalizedPageNumber,
                PageSize = normalizedPageSize
            };
        }

        public async Task<SubjectEnquiryPageResult<SubjectEnquiryPaymentItem>> GetPaymentsAsync(
            string subjectCode,
            int pageNumber,
            int pageSize,
            string? parentSubjectCode = null,
            CancellationToken cancellationToken = default)
        {
            var normalizedSubjectCode = NormalizeCode(subjectCode);
            var resolvedParentSubjectCode = await ResolveParentSubjectCodeAsync(
                normalizedSubjectCode,
                parentSubjectCode,
                cancellationToken);
            var (normalizedPageNumber, normalizedPageSize) = NormalizePaging(pageNumber, pageSize);

            var query =
                from payment in _nodeContext.Cash_tbPayments.AsNoTracking()
                join account in _nodeContext.Subject_tbAccounts.AsNoTracking()
                    on payment.AccountCode equals account.AccountCode
                join user in _nodeContext.Usr_tbUsers.AsNoTracking()
                    on payment.UserId equals user.UserId
                where payment.SubjectCode == normalizedSubjectCode
                   && (resolvedParentSubjectCode == null
                        ? payment.ParentSubjectCode == null
                        : payment.ParentSubjectCode == resolvedParentSubjectCode)
                orderby payment.PaidOn descending, payment.PaymentCode descending
                select new {
                    payment.PaymentCode,
                    payment.PaidOn,
                    payment.PaymentReference,
                    payment.PaidOutValue,
                    payment.PaidInValue,
                    account.AccountName,
                    user.UserName
                };

            var totalCount = await query.CountAsync(cancellationToken);

            var rows = await query
                .Skip((normalizedPageNumber - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .ToListAsync(cancellationToken);

            var items = rows
                .Select(o => new SubjectEnquiryPaymentItem(
                    o.PaymentCode,
                    o.PaidOn,
                    o.PaymentReference,
                    o.PaidOutValue,
                    o.PaidInValue,
                    o.AccountName,
                    o.UserName,
                    $"/Cash/Statement/Details?paymentCode={Uri.EscapeDataString(o.PaymentCode)}"))
                .ToList();

            return new SubjectEnquiryPageResult<SubjectEnquiryPaymentItem> {
                Items = items,
                TotalCount = totalCount,
                PageNumber = normalizedPageNumber,
                PageSize = normalizedPageSize
            };
        }

        public async Task<SubjectEnquiryPageResult<SubjectEnquiryStatementItem>> GetStatementAsync(
            string subjectCode,
            int pageNumber,
            int pageSize,
            string? parentSubjectCode = null,
            CancellationToken cancellationToken = default)
        {
            var normalizedSubjectCode = NormalizeCode(subjectCode);
            var resolvedParentSubjectCode = await ResolveParentSubjectCodeAsync(
                normalizedSubjectCode,
                parentSubjectCode,
                cancellationToken);
            var (normalizedPageNumber, normalizedPageSize) = NormalizePaging(pageNumber, pageSize);

            var query = _nodeContext.Subject_Statement
                .AsNoTracking()
                .Where(o => o.SubjectCode == normalizedSubjectCode)
                .Where(o => resolvedParentSubjectCode == null
                    ? o.ParentSubjectCode == null
                    : o.ParentSubjectCode == resolvedParentSubjectCode)
                .OrderByDescending(o => o.RowNumber);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((normalizedPageNumber - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .Select(o => new SubjectEnquiryStatementItem(
                    o.RowNumber,
                    o.TransactedOn,
                    o.Reference,
                    o.StatementType,
                    o.Charge,
                    o.Balance))
                .ToListAsync(cancellationToken);

            return new SubjectEnquiryPageResult<SubjectEnquiryStatementItem> {
                Items = items,
                TotalCount = totalCount,
                PageNumber = normalizedPageNumber,
                PageSize = normalizedPageSize
            };
        }

        private async Task<string?> ResolveParentSubjectCodeAsync(
            string subjectCode,
            string? parentSubjectCode,
            CancellationToken cancellationToken)
        {
            subjectCode = NormalizeCode(subjectCode);
            parentSubjectCode = NormalizeNullableCode(parentSubjectCode);

            if (string.IsNullOrWhiteSpace(subjectCode))
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(parentSubjectCode))
            {
                var exists = await _nodeContext.Subject_tbNamespaces
                    .AsNoTracking()
                    .AnyAsync(
                        o => o.ChildSubjectCode == subjectCode
                          && o.ParentSubjectCode == parentSubjectCode,
                        cancellationToken);

                if (!exists)
                {
                    throw new InvalidOperationException("The selected namespace could not be resolved for this subject enquiry.");
                }

                return parentSubjectCode;
            }

            var parents = await _nodeContext.Subject_tbNamespaces
                .AsNoTracking()
                .Where(o => o.ChildSubjectCode == subjectCode)
                .Select(o => o.ParentSubjectCode)
                .Distinct()
                .Take(2)
                .ToListAsync(cancellationToken);

            return parents.Count switch {
                0 => null,
                1 => parents[0],
                _ => throw new InvalidOperationException("A namespace must be selected for this subject enquiry.")
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            var normalizedPageNumber = Math.Max(pageNumber, 1);
            var normalizedPageSize = pageSize switch {
                <= 10 => 10,
                <= 25 => 25,
                _ => 50
            };

            return (normalizedPageNumber, normalizedPageSize);
        }

        private static string NormalizeCode(string? value)
        {
            return value?.Trim() ?? string.Empty;
        }

        private static string? NormalizeNullableCode(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
