using System.Collections.Generic;
using System.Threading.Tasks;
using TradeControl.Web.Pages.Invoice.Register.Models;
using TradeControl.Web.Pages.Subject.Controls;

namespace TradeControl.Web.AppServices.InvoiceRegister
{
    public interface IInvoiceRegisterWorkflowService
    {
        Task<InvoiceRaiseListResult> GetRaiseListAsync(short? invoiceTypeCode);
        Task<InvoiceRaiseEditModel> InitializeRaiseCreateAsync(short? invoiceTypeCode);
        Task<InvoiceRaiseEditModel> GetRaiseEditAsync(string entryId);
        Task<InvoiceRaiseEditModel> GetRaiseDetailsAsync(string entryId);
        Task<InvoiceRaiseEntrySummaryModel> GetRaiseDeleteAsync(string entryId);
        Task<InvoiceEntryPostModel> GetRaisePostAsync(string entryId);
        Task<InvoiceRaiseDefaultsModel?> GetRaiseDefaultsAsync(string subjectCode, string? parentSubjectCode = null, string? entryId = null, string? cashCode = null);
        Task<IReadOnlyList<NamespaceSelectorSuggestion>> GetNamespaceSuggestionsAsync(string filterText, int maxResults);
        Task<InvoiceWorkflowActionResult> SaveRaiseEntryAsync(InvoiceRaiseEditModel model);
        Task<InvoiceWorkflowActionResult> DeleteRaiseEntryAsync(string entryId);
        Task<InvoiceWorkflowActionResult> PostRaiseEntryAsync(InvoiceEntryPostModel model);
        Task<InvoiceWorkflowActionResult> PostRaiseAccountAsync(InvoiceEntryPostModel model);
        Task<InvoiceWorkflowActionResult> PostAllRaiseEntriesAsync();

        Task<InvoiceUpdateEditModel> GetUpdateEditAsync(string invoiceNumber);
        Task<InvoiceWorkflowActionResult> SaveUpdateHeaderAsync(InvoiceUpdateEditModel model);
        Task<InvoiceUpdateItemEditModel> InitializeCreateItemAsync(string invoiceNumber);
        Task<InvoiceUpdateItemEditModel> GetUpdateItemEditAsync(string invoiceNumber, string cashCode);
        Task<InvoiceRaiseEntrySummaryModel> GetUpdateItemDeleteAsync(string invoiceNumber, string cashCode);
        Task<InvoiceRaiseEntrySummaryModel> GetUpdateDeleteInvoiceAsync(string invoiceNumber);
        Task<InvoiceWorkflowActionResult> SaveUpdateItemAsync(InvoiceUpdateItemEditModel model);
        Task<InvoiceWorkflowActionResult> DeleteUpdateItemAsync(string invoiceNumber, string cashCode);
        Task<InvoiceWorkflowActionResult> DeleteUpdateInvoiceAsync(string invoiceNumber);

        Task<InvoiceSubmitModel> GetSubmitAsync(string invoiceNumber, string? emailAddress = null);
        Task<InvoiceSubmitPreviewModel> GetSubmitPreviewAsync(InvoiceSubmitModel model);
        Task<InvoiceWorkflowActionResult> SendSubmitAsync(InvoiceSubmitModel model);
        Task<InvoiceWorkflowActionResult> MarkInvoiceAsSentAsync(string invoiceNumber);
    }
}
