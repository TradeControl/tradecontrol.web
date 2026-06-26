using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Data;
using TradeControl.Web.Models;

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

            var currentBalance = await CreateStatementDagQuery(
                    normalizedSubjectCode,
                    resolvedParentSubjectCode)
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

            var query = CreateInvoiceDagQuery(
                    normalizedSubjectCode,
                    resolvedParentSubjectCode)
                .OrderByDescending(o => o.InvoicedOn)
                .ThenByDescending(o => o.InvoiceNumber);

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
                    $"/Invoice/Enquiry/Index?InvoiceNumber={Uri.EscapeDataString(o.InvoiceNumber)}"))
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

            var query = CreatePaymentDagQuery(
                    normalizedSubjectCode,
                    resolvedParentSubjectCode)
                .OrderByDescending(o => o.PaidOn)
                .ThenByDescending(o => o.PaymentCode);

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
                    $"/Cash/Manager?paymentCode={Uri.EscapeDataString(o.PaymentCode)}"))
                .ToList();

            return new SubjectEnquiryPageResult<SubjectEnquiryPaymentItem>
            {
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

            var query = CreateStatementDagQuery(
                normalizedSubjectCode,
                resolvedParentSubjectCode)
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

        private IQueryable<Subject_vwStatement> CreateStatementDagQuery(
            string subjectCode,
            string? parentSubjectCode)
        {
            return _nodeContext.Subject_Statement
                .FromSqlInterpolated(
                    $@"SELECT *
                       FROM Subject.fnStatementDag({subjectCode}, {parentSubjectCode})")
                .AsNoTracking();
        }

        private IQueryable<Subject_fnInvoiceDag> CreateInvoiceDagQuery(
            string subjectCode,
            string? parentSubjectCode)
        {
            return _nodeContext.Subject_InvoiceDag
                .FromSqlInterpolated(
                    $@"SELECT *
                       FROM Subject.fnInvoiceDag({subjectCode}, {parentSubjectCode})")
                .AsNoTracking();
        }

        private IQueryable<Subject_fnPaymentDag> CreatePaymentDagQuery(
            string subjectCode,
            string? parentSubjectCode)
        {
            return _nodeContext.Subject_PaymentDag
                .FromSqlInterpolated(
                    $@"SELECT *
                       FROM Subject.fnPaymentDag({subjectCode}, {parentSubjectCode})")
                .AsNoTracking();
        }
    }
}
