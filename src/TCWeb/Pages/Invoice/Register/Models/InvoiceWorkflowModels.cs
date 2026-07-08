using System;
using System.Collections.Generic;
using TradeControl.Web.Models;
using TradeControl.Web.Pages.Subject.Controls;

namespace TradeControl.Web.Pages.Invoice.Register.Models
{
    public sealed record InvoiceEntryKey(string EntryId, string AccountCode, string CashCode);

    public sealed record InvoiceUpdateItemKey(string InvoiceNumber, string CashCode);

    public sealed class InvoiceWorkflowActionResult
    {
        public bool Succeeded { get; init; }
        public string Message { get; init; } = string.Empty;

        public static InvoiceWorkflowActionResult Success(string message)
            => new() { Succeeded = true, Message = message };

        public static InvoiceWorkflowActionResult Failure(string message)
            => new() { Succeeded = false, Message = message };
    }

    public sealed class InvoiceRaiseDefaultsModel
    {
        public string SubjectCode { get; set; } = string.Empty;
        public string ParentSubjectCode { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public short InvoiceTypeCode { get; set; }
        public string CashCode { get; set; } = string.Empty;
        public decimal TotalValue { get; set; }
        public decimal InvoiceValue { get; set; }
        public string ItemReference { get; set; } = string.Empty;
    }

    public sealed class InvoiceRaiseListResult
    {
        public List<Invoice_vwEntry> Entries { get; set; } = [];
        public short? InvoiceTypeCode { get; set; }
    }

    public sealed class InvoiceRaiseEntrySummaryModel
    {
        public string EntryId { get; set; } = string.Empty;
        public string InvoiceNumberOrAccountDisplay { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string ParentSubjectCode { get; set; } = string.Empty;
        public string NamespacePath { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string CashCode { get; set; } = string.Empty;
        public string CashDescription { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string TaxDescription { get; set; } = string.Empty;
        public short InvoiceTypeCode { get; set; }
        public string InvoiceType { get; set; } = string.Empty;
        public DateTime InvoicedOn { get; set; }
        public decimal TotalValue { get; set; }
        public decimal InvoiceValue { get; set; }
        public string ItemReference { get; set; } = string.Empty;
    }

    public sealed class InvoiceRaiseEditModel
    {
        public bool IsEditMode { get; set; }
        public string EntryId { get; set; } = string.Empty;
        public string OriginalEntryId { get; set; } = string.Empty;
        public string OriginalAccountCode { get; set; } = string.Empty;
        public string OriginalCashCode { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string ParentSubjectCode { get; set; } = string.Empty;
        public string NamespacePath { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string CashCode { get; set; } = string.Empty;
        public string CashDescription { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public short InvoiceTypeCode { get; set; }
        public DateTime InvoicedOn { get; set; } = DateTime.Today;
        public decimal TotalValue { get; set; }
        public decimal InvoiceValue { get; set; }
        public string ItemReference { get; set; } = string.Empty;
        public InvoiceRaiseEntrySummaryModel? EntryHeader { get; set; }

        public IReadOnlyList<InvoiceRegisterSelectOption> CashCodeOptions { get; set; } = Array.Empty<InvoiceRegisterSelectOption>();
        public IReadOnlyList<InvoiceRegisterSelectOption> TaxCodeOptions { get; set; } = Array.Empty<InvoiceRegisterSelectOption>();
        public IReadOnlyList<InvoiceRegisterInvoiceTypeOption> InvoiceTypeOptions { get; set; } = Array.Empty<InvoiceRegisterInvoiceTypeOption>();
        public IReadOnlyList<NamespaceSelectorSuggestion> NamespaceSuggestions { get; set; } = Array.Empty<NamespaceSelectorSuggestion>();
    }

    public sealed class InvoiceEntryPostModel
    {
        public string EntryId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string ParentSubjectCode { get; set; } = string.Empty;
        public string NamespacePath { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string CashCode { get; set; } = string.Empty;
        public string CashDescription { get; set; } = string.Empty;
        public short InvoiceTypeCode { get; set; }
        public string InvoiceType { get; set; } = string.Empty;
        public DateTime InvoicedOn { get; set; }
        public decimal TotalValue { get; set; }
        public decimal InvoiceValue { get; set; }
        public string TaxCode { get; set; } = string.Empty;
        public string TaxDescription { get; set; } = string.Empty;
        public string ItemReference { get; set; } = string.Empty;
    }

    public sealed class InvoiceUpdateEditModel
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string ParentSubjectCode { get; set; } = string.Empty;
        public string NamespacePath { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public short InvoiceTypeCode { get; set; }
        public short InvoiceStatusCode { get; set; }
        public DateTime InvoicedOn { get; set; }
        public DateTime DueOn { get; set; }
        public DateTime ExpectedOn { get; set; }
        public string PaymentTerms { get; set; } = string.Empty;
        public bool Printed { get; set; }
        public string Notes { get; set; } = string.Empty;

        public Invoice_vwRegister? Header { get; set; }
        public List<Invoice_vwRegisterDetail> Details { get; set; } = [];
        public IReadOnlyList<InvoiceRegisterInvoiceTypeOption> InvoiceTypeOptions { get; set; } = Array.Empty<InvoiceRegisterInvoiceTypeOption>();
        public IReadOnlyList<InvoiceRegisterInvoiceStatusOption> InvoiceStatusOptions { get; set; } = Array.Empty<InvoiceRegisterInvoiceStatusOption>();
    }

    public sealed class InvoiceUpdateItemEditModel
    {
        public bool IsEditMode { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string OriginalCashCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string NamespacePath { get; set; } = string.Empty;
        public string CashCode { get; set; } = string.Empty;
        public string CashDescription { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public decimal TotalValue { get; set; }
        public decimal InvoiceValue { get; set; }
        public string ItemReference { get; set; } = string.Empty;

        public IReadOnlyList<InvoiceRegisterSelectOption> CashCodeOptions { get; set; } = Array.Empty<InvoiceRegisterSelectOption>();
        public IReadOnlyList<InvoiceRegisterSelectOption> TaxCodeOptions { get; set; } = Array.Empty<InvoiceRegisterSelectOption>();
    }

    public sealed class InvoiceDeleteConfirmationModel
    {
        public string Title { get; set; } = string.Empty;
        public string EntryId { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string NamespacePath { get; set; } = string.Empty;
        public string Primary { get; set; } = string.Empty;
        public string Secondary { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string AccountCode { get; set; } = string.Empty;
        public string ParentSubjectCode { get; set; } = string.Empty;
        public string CashCode { get; set; } = string.Empty;
    }
}
