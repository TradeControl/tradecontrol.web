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
            CancellationToken cancellationToken = default)
        {
            var summary = await (
                from subject in _nodeContext.Subject_tbSubjects.AsNoTracking()
                join subjectType in _nodeContext.Subject_tbTypes.AsNoTracking()
                    on subject.SubjectTypeCode equals subjectType.SubjectTypeCode
                where subject.SubjectCode == subjectCode
                select new {
                    subject.SubjectCode,
                    subject.SubjectName,
                    subjectType.SubjectType,
                    subjectType.CashPolarityCode
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (summary is null)
                return null;

            var currentBalance = await _nodeContext.Subject_Statement
                .AsNoTracking()
                .Where(o => o.SubjectCode == subjectCode)
                .OrderByDescending(o => o.RowNumber)
                .Select(o => (double?)o.Balance)
                .FirstOrDefaultAsync(cancellationToken) ?? 0d;

            return new SubjectEnquirySummary(
                summary.SubjectCode,
                summary.SubjectName,
                summary.SubjectType,
                (NodeEnum.CashPolarity)summary.CashPolarityCode,
                currentBalance);
        }

        public async Task<SubjectEnquiryPageResult<SubjectEnquiryInvoiceItem>> GetInvoicesAsync(
            string subjectCode,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var (normalizedPageNumber, normalizedPageSize) = NormalizePaging(pageNumber, pageSize);

            var query = _nodeContext.Invoice_Register
                .AsNoTracking()
                .Where(o => o.SubjectCode == subjectCode)
                .OrderByDescending(o => o.InvoicedOn)
                .ThenByDescending(o => o.InvoiceNumber);

            var totalCount = await query.CountAsync(cancellationToken);

            var rows = await query
                .Skip((normalizedPageNumber - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .Select(o => new {
                    o.InvoiceNumber,
                    o.InvoiceType,
                    o.InvoicedOn,
                    o.InvoiceValue,
                    o.TaxValue,
                    o.TotalPaidValue,
                    o.InvoiceStatus
                })
                .ToListAsync(cancellationToken);

            var items = rows
                .Select(o => new SubjectEnquiryInvoiceItem(
                    o.InvoiceNumber,
                    o.InvoiceType,
                    o.InvoicedOn,
                    o.InvoiceValue,
                    o.TaxValue,
                    o.TotalPaidValue,
                    o.InvoiceStatus,
                    $"/Invoice/Register/Index?InvoiceNumber={Uri.EscapeDataString(o.InvoiceNumber)}"))
                .ToList();

            return new SubjectEnquiryPageResult<SubjectEnquiryInvoiceItem> {
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
            CancellationToken cancellationToken = default)
        {
            var (normalizedPageNumber, normalizedPageSize) = NormalizePaging(pageNumber, pageSize);

            var query = _nodeContext.Cash_Payments
                .AsNoTracking()
                .Where(o => o.SubjectCode == subjectCode)
                .OrderByDescending(o => o.PaidOn)
                .ThenByDescending(o => o.PaymentCode);

            var totalCount = await query.CountAsync(cancellationToken);

            var rows = await query
                .Skip((normalizedPageNumber - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .Select(o => new {
                    o.PaymentCode,
                    o.PaidOn,
                    o.PaymentReference,
                    o.PaidOutValue,
                    o.PaidInValue,
                    o.AccountName,
                    o.UserName
                })
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
            CancellationToken cancellationToken = default)
        {
            var (normalizedPageNumber, normalizedPageSize) = NormalizePaging(pageNumber, pageSize);

            var query = _nodeContext.Subject_Statement
                .AsNoTracking()
                .Where(o => o.SubjectCode == subjectCode)
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
    }
}
