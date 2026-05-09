using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradeControl.Web.Data;

namespace TradeControl.Web.AppServices
{
    public enum SubjectEnquiryView
    {
        None,
        Invoices,
        Payments,
        Statement
    }

    public sealed class SubjectEnquiryPageResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
    }

    public sealed record SubjectEnquirySummary(
        string SubjectCode,
        string SubjectName,
        string SubjectType,
        NodeEnum.CashPolarity CashPolarity,
        double CurrentBalance);

    public sealed record SubjectEnquiryInvoiceItem(
        string InvoiceNumber,
        string InvoiceType,
        DateTime InvoicedOn,
        double InvoiceValue,
        double TaxValue,
        double TotalPaidValue,
        string InvoiceStatus,
        string RegisterUrl);

    public sealed record SubjectEnquiryPaymentItem(
        string PaymentCode,
        DateTime PaidOn,
        string? PaymentReference,
        decimal PaidOutValue,
        decimal PaidInValue,
        string AccountName,
        string UserName,
        string DetailsUrl);

    public sealed record SubjectEnquiryStatementItem(
        int RowNumber,
        DateTime TransactedOn,
        string? Reference,
        string? StatementType,
        double Charge,
        double Balance);

    public interface ISubjectEnquiryService
    {
        Task<SubjectEnquirySummary?> GetSummaryAsync(
            string subjectCode,
            CancellationToken cancellationToken = default);

        Task<SubjectEnquiryPageResult<SubjectEnquiryInvoiceItem>> GetInvoicesAsync(
            string subjectCode,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<SubjectEnquiryPageResult<SubjectEnquiryPaymentItem>> GetPaymentsAsync(
            string subjectCode,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<SubjectEnquiryPageResult<SubjectEnquiryStatementItem>> GetStatementAsync(
            string subjectCode,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
