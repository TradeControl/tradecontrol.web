using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using TradeControl.Web.Data;
using TradeControl.Web.Mail;
using TradeControl.Web.Models;
using TradeControl.Web.Pages.Invoice.Register.Models;
using TradeControl.Web.Pages.Subject.Controls;

namespace TradeControl.Web.AppServices.InvoiceRegister
{
    public sealed class InvoiceRegisterWorkflowService : IInvoiceRegisterWorkflowService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InvoiceRegisterWorkflowService(IServiceScopeFactory scopeFactory, IHttpContextAccessor httpContextAccessor)
        {
            _scopeFactory = scopeFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<InvoiceRaiseListResult> GetRaiseListAsync(short? invoiceTypeCode)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var query = nodeContext.Invoice_Entries.AsNoTracking();

            if (invoiceTypeCode.HasValue)
                query = query.Where(entry => entry.InvoiceTypeCode == invoiceTypeCode.Value);

            var entries = await query
                .OrderBy(entry => entry.InvoicedOn)
                .ThenBy(entry => entry.SubjectName)
                .ThenBy(entry => entry.CashDescription)
                .ToListAsync();

            return new InvoiceRaiseListResult
            {
                Entries = entries,
                InvoiceTypeCode = invoiceTypeCode
            };
        }

        public async Task<InvoiceRaiseEditModel> InitializeRaiseCreateAsync(short? invoiceTypeCode)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var profile = new Profile(nodeContext);

            var subjectCode = await profile.CompanySubjectCode();
            var subject = string.IsNullOrWhiteSpace(subjectCode)
                ? null
                : await nodeContext.Subject_tbSubjects
                    .AsNoTracking()
                    .FirstOrDefaultAsync(item => item.SubjectCode == subjectCode);

            var cashCodeLookup = await nodeContext.Cash_CodeLookup
                .AsNoTracking()
                .Where(code => code.CashTypeCode < (short)NodeEnum.CashType.Money)
                .OrderBy(code => code.CashCode)
                .FirstOrDefaultAsync();

            var taxCode = await nodeContext.App_tbTaxCodes
                .AsNoTracking()
                .Where(tax => tax.TaxTypeCode == (short)NodeEnum.TaxType.VAT)
                .OrderBy(tax => tax.TaxCode)
                .Select(tax => tax.TaxCode)
                .FirstOrDefaultAsync() ?? string.Empty;

            var invoiceType = invoiceTypeCode ?? (short)NodeEnum.InvoiceType.SalesInvoice;
            var parentSubjectCode = await GetDefaultParentSubjectCodeAsync(nodeContext, subjectCode);

            return new InvoiceRaiseEditModel
            {
                SubjectCode = subjectCode,
                ParentSubjectCode = parentSubjectCode,
                NamespacePath = BuildNamespacePath(parentSubjectCode, subjectCode),
                SubjectName = subject?.SubjectName ?? string.Empty,
                CashCode = cashCodeLookup?.CashCode ?? string.Empty,
                CashDescription = cashCodeLookup?.CashDescription ?? string.Empty,
                TaxCode = taxCode,
                InvoiceTypeCode = invoiceType,
                InvoicedOn = DateTime.Today,
                CashCodeOptions = await GetCashCodeOptionsAsync(nodeContext),
                TaxCodeOptions = await GetTaxCodeOptionsAsync(nodeContext),
                InvoiceTypeOptions = await GetInvoiceTypeOptionsAsync(nodeContext)
            };
        }

        public async Task<InvoiceRaiseEditModel> GetRaiseEditAsync(string entryId)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var header = await nodeContext.Invoice_Entries
                .AsNoTracking()
                .FirstAsync(entry => entry.EntryId == entryId);

            var entry = await nodeContext.Invoice_tbEntries
                .AsNoTracking()
                .FirstAsync(item => item.EntryId == entryId);

            var parentSubjectCode = string.IsNullOrWhiteSpace(entry.ParentSubjectCode)
                ? await GetDefaultParentSubjectCodeAsync(nodeContext, entry.SubjectCode)
                : entry.ParentSubjectCode;

            return new InvoiceRaiseEditModel
            {
                IsEditMode = true,
                EntryId = entry.EntryId,
                OriginalEntryId = entry.EntryId,
                OriginalAccountCode = entry.SubjectCode,
                OriginalCashCode = entry.CashCode,
                SubjectCode = entry.SubjectCode,
                ParentSubjectCode = parentSubjectCode,
                NamespacePath = BuildNamespacePath(parentSubjectCode, entry.SubjectCode),
                SubjectName = header.SubjectName,
                CashCode = entry.CashCode,
                CashDescription = header.CashDescription,
                TaxCode = entry.TaxCode,
                InvoiceTypeCode = entry.InvoiceTypeCode,
                InvoicedOn = entry.InvoicedOn,
                TotalValue = entry.TotalValue,
                InvoiceValue = entry.InvoiceValue,
                ItemReference = entry.ItemReference,
                EntryHeader = await ToSummaryAsync(nodeContext, header),
                CashCodeOptions = await GetCashCodeOptionsAsync(nodeContext),
                TaxCodeOptions = await GetTaxCodeOptionsAsync(nodeContext),
                InvoiceTypeOptions = await GetInvoiceTypeOptionsAsync(nodeContext)
            };
        }

        public async Task<InvoiceRaiseEditModel> GetRaiseDetailsAsync(string entryId)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var header = await nodeContext.Invoice_Entries
                .AsNoTracking()
                .FirstAsync(entry => entry.EntryId == entryId);

            return new InvoiceRaiseEditModel
            {
                EntryHeader = await ToSummaryAsync(nodeContext, header)
            };
        }

        public async Task<InvoiceRaiseEntrySummaryModel> GetRaiseDeleteAsync(string entryId)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var header = await nodeContext.Invoice_Entries
                .AsNoTracking()
                .FirstAsync(entry => entry.EntryId == entryId);

            return await ToSummaryAsync(nodeContext, header);
        }

        public async Task<InvoiceEntryPostModel> GetRaisePostAsync(string entryId)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var header = await nodeContext.Invoice_Entries
                .AsNoTracking()
                .FirstAsync(entry => entry.EntryId == entryId);

            var parentSubjectCode = string.IsNullOrWhiteSpace(header.ParentSubjectCode)
                ? await GetDefaultParentSubjectCodeAsync(nodeContext, header.SubjectCode)
                : header.ParentSubjectCode;

            return new InvoiceEntryPostModel
            {
                EntryId = header.EntryId,
                SubjectCode = header.SubjectCode,
                ParentSubjectCode = parentSubjectCode,
                NamespacePath = BuildNamespacePath(parentSubjectCode, header.SubjectCode),
                SubjectName = header.SubjectName,
                CashCode = header.CashCode,
                CashDescription = header.CashDescription,
                InvoiceTypeCode = header.InvoiceTypeCode,
                InvoiceType = header.InvoiceType,
                InvoicedOn = header.InvoicedOn,
                TotalValue = header.TotalValue,
                InvoiceValue = header.InvoiceValue,
                TaxCode = header.TaxCode,
                TaxDescription = header.TaxDescription,
                ItemReference = header.ItemReference,
                UserId = header.UserId
            };
        }

        public async Task<InvoiceRaiseDefaultsModel?> GetRaiseDefaultsAsync(string subjectCode, string? parentSubjectCode = null, string? entryId = null, string? cashCode = null)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();
            var invoices = new Invoices(nodeContext);

            if (string.IsNullOrWhiteSpace(subjectCode))
                return null;

            return await invoices.RaiseDefaults(subjectCode, parentSubjectCode, entryId, cashCode);
        }

        public async Task<IReadOnlyList<NamespaceSelectorSuggestion>> GetNamespaceSuggestionsAsync(string filterText, int maxResults)
        {
            using var scope = _scopeFactory.CreateScope();
            var subjectBrowserService = scope.ServiceProvider.GetRequiredService<ISubjectBrowserService>();

            return await subjectBrowserService.GetNamespaceSuggestionsAsync(filterText, maxResults);
        }

        public async Task<InvoiceWorkflowActionResult> SaveRaiseEntryAsync(InvoiceRaiseEditModel model)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            ApplyNamespaceSelection(model);

            var subjectCode = model.SubjectCode;
            var parentSubjectCode = string.IsNullOrWhiteSpace(model.ParentSubjectCode) ? null : model.ParentSubjectCode;
            var cashCode = model.CashCode;
            var taxCode = model.TaxCode;

            if (model.TotalValue != 0 && model.InvoiceValue != 0)
                model.InvoiceValue = 0;

            if (model.IsEditMode)
            {
                var rowsAffected = await nodeContext.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE Invoice.tbEntry
SET
    SubjectCode = {subjectCode},
    ParentSubjectCode = {parentSubjectCode},
    CashCode = {cashCode},
    TaxCode = {taxCode},
    InvoiceTypeCode = {model.InvoiceTypeCode},
    InvoicedOn = {model.InvoicedOn},
    TotalValue = {model.TotalValue},
    InvoiceValue = {model.InvoiceValue},
    ItemReference = {model.ItemReference}
WHERE EntryId = {model.OriginalEntryId}");

                if (rowsAffected == 0)
                    return InvoiceWorkflowActionResult.Failure("Pending entry not found.");

                if (rowsAffected > 1)
                    return InvoiceWorkflowActionResult.Failure("Pending entry update affected multiple rows.");

                model.EntryId = model.OriginalEntryId;

                return InvoiceWorkflowActionResult.Success("Pending entry updated.");
            }

            var profile = new Profile(nodeContext);
            var userId = await nodeContext.Usr_tbUsers
                .AsNoTracking()
                .OrderBy(user => user.UserId)
                .Select(user => user.UserId)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(userId))
                userId = await profile.UserId(string.Empty);

            var invoices = new Invoices(nodeContext);
            var entryId = await invoices.DefaultEntryCode(userId ?? string.Empty);

            if (string.IsNullOrWhiteSpace(entryId))
                return InvoiceWorkflowActionResult.Failure("Unable to generate a pending entry identifier.");

            var newEntry = new Invoice_tbEntry
            {
                EntryId = entryId,
                UserId = userId ?? string.Empty,
                SubjectCode = subjectCode,
                ParentSubjectCode = parentSubjectCode,
                CashCode = cashCode,
                InvoiceTypeCode = model.InvoiceTypeCode,
                InvoicedOn = model.InvoicedOn,
                TaxCode = taxCode,
                ItemReference = model.ItemReference,
                TotalValue = model.TotalValue,
                InvoiceValue = model.InvoiceValue
            };

            nodeContext.Invoice_tbEntries.Add(newEntry);
            await nodeContext.SaveChangesAsync();

            model.EntryId = entryId;

            return InvoiceWorkflowActionResult.Success("Pending entry created.");
        }

        public async Task<InvoiceWorkflowActionResult> DeleteRaiseEntryAsync(string entryId)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var entity = await nodeContext.Invoice_tbEntries
                .FirstOrDefaultAsync(entry => entry.EntryId == entryId);

            if (entity is null)
                return InvoiceWorkflowActionResult.Failure("Pending entry not found.");

            nodeContext.Invoice_tbEntries.Remove(entity);
            await nodeContext.SaveChangesAsync();

            return InvoiceWorkflowActionResult.Success("Pending entry deleted.");
        }

        public async Task<InvoiceWorkflowActionResult> PostRaiseEntryAsync(InvoiceEntryPostModel model)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();
            var invoices = new Invoices(nodeContext);

            var entry = await nodeContext.Invoice_tbEntries
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.EntryId == model.EntryId);

            if (entry is null)
                return InvoiceWorkflowActionResult.Failure("Pending entry not found.");

            var success = await invoices.PostByEntry(
                model.UserId,
                entry.EntryId,
                string.IsNullOrWhiteSpace(entry.ParentSubjectCode) ? model.ParentSubjectCode : entry.ParentSubjectCode);

            if (!success)
                return InvoiceWorkflowActionResult.Failure("Unable to post the selected entry.");

            return InvoiceWorkflowActionResult.Success(BuildPostSuccessMessage(model.InvoiceTypeCode));
        }

        public async Task<InvoiceWorkflowActionResult> PostRaiseAccountAsync(InvoiceEntryPostModel model)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();
            var invoices = new Invoices(nodeContext);

            var success = await invoices.PostByAccount(
                model.UserId,
                model.SubjectCode,
                model.ParentSubjectCode);

            if (!success)
                return InvoiceWorkflowActionResult.Failure("Unable to post account entries.");

            return InvoiceWorkflowActionResult.Success(BuildPostSuccessMessage(model.InvoiceTypeCode));
        }

        public async Task<InvoiceWorkflowActionResult> PostAllRaiseEntriesAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();
            var invoices = new Invoices(nodeContext);

            var entriesByUser = await nodeContext.Invoice_Entries
                .AsNoTracking()
                .GroupBy(entry => entry.UserId)
                .Select(group => new
                {
                    UserId = group.Key,
                    HasEmailedInvoices = group.Any(entry =>
                        entry.InvoiceTypeCode == (short)NodeEnum.InvoiceType.SalesInvoice ||
                        entry.InvoiceTypeCode == (short)NodeEnum.InvoiceType.CreditNote)
                })
                .ToListAsync();

            var hasEmailWorkflow = false;

            foreach (var item in entriesByUser)
            {
                if (!await invoices.Post(item.UserId))
                    return InvoiceWorkflowActionResult.Failure("Unable to post one or more pending entries.");

                if (item.HasEmailedInvoices)
                    hasEmailWorkflow = true;
            }

            return InvoiceWorkflowActionResult.Success(hasEmailWorkflow
                ? "All pending entries posted. Sales invoices and credit notes remain available for submission."
                : "All pending entries posted.");
        }

        public async Task<InvoiceUpdateEditModel> GetUpdateEditAsync(string invoiceNumber)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var header = await nodeContext.Invoice_Register
                .AsNoTracking()
                .FirstAsync(invoice => invoice.InvoiceNumber == invoiceNumber);

            var invoice = await nodeContext.Invoice_tbInvoices
                .AsNoTracking()
                .FirstAsync(item => item.InvoiceNumber == invoiceNumber);

            var details = await nodeContext.Invoice_RegisterDetails
                .AsNoTracking()
                .Where(detail => detail.InvoiceNumber == invoiceNumber)
                .OrderBy(detail => detail.ProjectCode)
                .ThenBy(detail => detail.CashCode)
                .ToListAsync();

            return new InvoiceUpdateEditModel
            {
                InvoiceNumber = invoice.InvoiceNumber,
                SubjectCode = invoice.SubjectCode,
                ParentSubjectCode = invoice.ParentSubjectCode ?? string.Empty,
                NamespacePath = BuildNamespacePath(invoice.ParentSubjectCode, invoice.SubjectCode),
                SubjectName = header.SubjectName,
                UserId = invoice.UserId,
                InvoiceTypeCode = invoice.InvoiceTypeCode,
                InvoiceStatusCode = invoice.InvoiceStatusCode,
                InvoicedOn = invoice.InvoicedOn,
                DueOn = invoice.DueOn,
                ExpectedOn = invoice.ExpectedOn,
                PaymentTerms = invoice.PaymentTerms,
                Printed = invoice.Printed,
                Notes = invoice.Notes,
                Header = header,
                Details = details,
                InvoiceTypeOptions = await GetInvoiceTypeOptionsAsync(nodeContext),
                InvoiceStatusOptions = await GetInvoiceStatusOptionsAsync(nodeContext)
            };
        }

        public async Task<InvoiceWorkflowActionResult> SaveUpdateHeaderAsync(InvoiceUpdateEditModel model)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var entity = await nodeContext.Invoice_tbInvoices
                .FirstAsync(invoice => invoice.InvoiceNumber == model.InvoiceNumber);

            var previousInvoicedOn = entity.InvoicedOn;
            var previousInvoiceTypeCode = entity.InvoiceTypeCode;
            var previousInvoiceStatusCode = entity.InvoiceStatusCode;

            entity.InvoiceTypeCode = model.InvoiceTypeCode;
            entity.InvoiceStatusCode = model.InvoiceStatusCode;
            entity.InvoicedOn = model.InvoicedOn;
            entity.DueOn = model.DueOn;
            entity.ExpectedOn = model.ExpectedOn;
            entity.PaymentTerms = model.PaymentTerms;
            entity.Printed = model.Printed;
            entity.Notes = model.Notes;

            await nodeContext.SaveChangesAsync();

            var invoices = new Invoices(nodeContext, entity.InvoiceNumber);
            await invoices.Accept();

            var periods = new FinancialPeriods(nodeContext);
            var activePeriod = periods.ActiveStartOn;
            var periodRebuild = previousInvoicedOn != entity.InvoicedOn
                && (previousInvoicedOn < activePeriod || entity.InvoicedOn < activePeriod);

            if (periodRebuild)
                await periods.Generate();

            var orgRebuild = previousInvoiceTypeCode != entity.InvoiceTypeCode
                || previousInvoiceStatusCode != entity.InvoiceStatusCode
                || periodRebuild;

            if (orgRebuild)
            {
                var subjects = new Subjects(nodeContext, entity.SubjectCode);
                await subjects.Rebuild();
            }

            return InvoiceWorkflowActionResult.Success("Invoice header updated.");
        }

        public async Task<InvoiceUpdateItemEditModel> InitializeCreateItemAsync(string invoiceNumber)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var header = await nodeContext.Invoice_Register
                .AsNoTracking()
                .FirstAsync(invoice => invoice.InvoiceNumber == invoiceNumber);

            var cashMode = ResolveCashPolarity((NodeEnum.InvoiceType)header.InvoiceTypeCode);

            var cashCodeLookup = await nodeContext.Cash_CodeLookup
                .AsNoTracking()
                .Where(code => code.CashTypeCode < (short)NodeEnum.CashType.Money && code.CashPolarityCode == (short)cashMode)
                .OrderBy(code => code.CashCode)
                .FirstOrDefaultAsync();

            var taxCode = await nodeContext.App_tbTaxCodes
                .AsNoTracking()
                .Where(tax => tax.TaxTypeCode == (short)NodeEnum.TaxType.VAT)
                .OrderBy(tax => tax.TaxCode)
                .Select(tax => tax.TaxCode)
                .FirstOrDefaultAsync() ?? string.Empty;

            return new InvoiceUpdateItemEditModel
            {
                InvoiceNumber = invoiceNumber,
                SubjectName = header.SubjectName,
                NamespacePath = BuildNamespacePath(header.ParentSubjectCode, header.SubjectCode),
                CashCode = cashCodeLookup?.CashCode ?? string.Empty,
                CashDescription = cashCodeLookup?.CashDescription ?? string.Empty,
                TaxCode = taxCode,
                TotalValue = 0,
                InvoiceValue = 0,
                ItemReference = string.Empty,
                CashCodeOptions = await GetCashCodeOptionsByPolarityAsync(nodeContext, cashMode),
                TaxCodeOptions = await GetTaxCodeOptionsAsync(nodeContext)
            };
        }

        public async Task<InvoiceUpdateItemEditModel> GetUpdateItemEditAsync(string invoiceNumber, string cashCode)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var detail = await nodeContext.Invoice_RegisterDetails
                .AsNoTracking()
                .FirstAsync(item => item.InvoiceNumber == invoiceNumber && item.CashCode == cashCode);

            var entity = await nodeContext.Invoice_tbItems
                .AsNoTracking()
                .FirstAsync(item => item.InvoiceNumber == invoiceNumber && item.CashCode == cashCode);

            return new InvoiceUpdateItemEditModel
            {
                IsEditMode = true,
                OriginalCashCode = entity.CashCode,
                InvoiceNumber = entity.InvoiceNumber,
                SubjectName = detail.SubjectName,
                NamespacePath = BuildNamespacePath(detail.ParentSubjectCode, detail.SubjectCode),
                CashCode = entity.CashCode,
                CashDescription = detail.CashDescription,
                TaxCode = entity.TaxCode,
                TotalValue = entity.TotalValue,
                InvoiceValue = entity.InvoiceValue,
                ItemReference = entity.ItemReference,
                CashCodeOptions = await GetCashCodeOptionsAsync(nodeContext),
                TaxCodeOptions = await GetTaxCodeOptionsAsync(nodeContext)
            };
        }

        public async Task<InvoiceRaiseEntrySummaryModel> GetUpdateItemDeleteAsync(string invoiceNumber, string cashCode)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var detail = await nodeContext.Invoice_RegisterDetails
                .AsNoTracking()
                .FirstAsync(item => item.InvoiceNumber == invoiceNumber && item.CashCode == cashCode);

            return new InvoiceRaiseEntrySummaryModel
            {
                InvoiceNumberOrAccountDisplay = detail.InvoiceNumber,
                SubjectCode = detail.SubjectCode,
                ParentSubjectCode = detail.ParentSubjectCode ?? string.Empty,
                NamespacePath = BuildNamespacePath(detail.ParentSubjectCode, detail.SubjectCode),
                SubjectName = detail.SubjectName,
                CashCode = detail.CashCode,
                CashDescription = detail.CashDescription,
                TaxCode = detail.TaxCode,
                TaxDescription = detail.TaxDescription,
                InvoiceTypeCode = detail.InvoiceTypeCode,
                InvoiceType = detail.InvoiceType,
                InvoicedOn = detail.InvoicedOn,
                TotalValue = (decimal)detail.TotalValue,
                InvoiceValue = (decimal)detail.InvoiceValue,
                ItemReference = detail.ItemReference
            };
        }

        public async Task<InvoiceRaiseEntrySummaryModel> GetUpdateDeleteInvoiceAsync(string invoiceNumber)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var header = await nodeContext.Invoice_Register
                .AsNoTracking()
                .FirstAsync(invoice => invoice.InvoiceNumber == invoiceNumber);

            return new InvoiceRaiseEntrySummaryModel
            {
                InvoiceNumberOrAccountDisplay = header.InvoiceNumber,
                SubjectCode = header.SubjectCode,
                ParentSubjectCode = header.ParentSubjectCode ?? string.Empty,
                NamespacePath = BuildNamespacePath(header.ParentSubjectCode, header.SubjectCode),
                SubjectName = header.SubjectName,
                InvoiceTypeCode = header.InvoiceTypeCode,
                InvoiceType = header.InvoiceType,
                InvoicedOn = header.InvoicedOn,
                TotalValue = (decimal)header.TotalInvoiceValue,
                InvoiceValue = (decimal)header.InvoiceValue,
                ItemReference = header.Notes
            };
        }

        public async Task<InvoiceWorkflowActionResult> SaveUpdateItemAsync(InvoiceUpdateItemEditModel model)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            if (model.TotalValue != 0 && model.InvoiceValue != 0)
                model.InvoiceValue = 0;

            Invoice_tbItem entity;

            if (model.IsEditMode)
            {
                entity = await nodeContext.Invoice_tbItems
                    .FirstAsync(item => item.InvoiceNumber == model.InvoiceNumber && item.CashCode == model.OriginalCashCode);

                entity.CashCode = model.CashCode;
                entity.TaxCode = model.TaxCode;
                entity.TotalValue = model.TotalValue;
                entity.InvoiceValue = model.InvoiceValue;
                entity.ItemReference = model.ItemReference;
            }
            else
            {
                entity = new Invoice_tbItem
                {
                    InvoiceNumber = model.InvoiceNumber,
                    CashCode = model.CashCode,
                    TaxCode = model.TaxCode,
                    TotalValue = model.TotalValue,
                    InvoiceValue = model.InvoiceValue,
                    ItemReference = model.ItemReference
                };

                nodeContext.Invoice_tbItems.Add(entity);
            }

            await nodeContext.SaveChangesAsync();

            var invoices = new Invoices(nodeContext, model.InvoiceNumber);
            await invoices.Accept();

            var invoiceHeader = await nodeContext.Invoice_tbInvoices
                .AsNoTracking()
                .FirstAsync(invoice => invoice.InvoiceNumber == model.InvoiceNumber);

            var periods = new FinancialPeriods(nodeContext);
            if (invoiceHeader.InvoicedOn < periods.ActiveStartOn)
                await periods.Generate();

            var subjects = new Subjects(nodeContext, invoiceHeader.SubjectCode);
            await subjects.Rebuild();

            return InvoiceWorkflowActionResult.Success(model.IsEditMode
                ? "Invoice item updated."
                : "Invoice item created.");
        }

        public async Task<InvoiceWorkflowActionResult> DeleteUpdateItemAsync(string invoiceNumber, string cashCode)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var entity = await nodeContext.Invoice_tbItems
                .FirstOrDefaultAsync(item => item.InvoiceNumber == invoiceNumber && item.CashCode == cashCode);

            if (entity is null)
                return InvoiceWorkflowActionResult.Failure("Invoice item not found.");

            nodeContext.Invoice_tbItems.Remove(entity);
            await nodeContext.SaveChangesAsync();

            var invoices = new Invoices(nodeContext, invoiceNumber);
            await invoices.Accept();

            var invoiceHeader = await nodeContext.Invoice_tbInvoices
                .AsNoTracking()
                .FirstAsync(invoice => invoice.InvoiceNumber == invoiceNumber);

            var periods = new FinancialPeriods(nodeContext);
            if (invoiceHeader.InvoicedOn < periods.ActiveStartOn)
                await periods.Generate();

            var subjects = new Subjects(nodeContext, invoiceHeader.SubjectCode);
            await subjects.Rebuild();

            return InvoiceWorkflowActionResult.Success("Invoice item deleted.");
        }

        public async Task<InvoiceWorkflowActionResult> DeleteUpdateInvoiceAsync(string invoiceNumber)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var entity = await nodeContext.Invoice_tbInvoices
                .FirstOrDefaultAsync(invoice => invoice.InvoiceNumber == invoiceNumber);

            if (entity is null)
                return InvoiceWorkflowActionResult.Failure("Invoice not found.");

            var userId = entity.UserId;
            var subjectCode = entity.SubjectCode;

            nodeContext.Invoice_tbInvoices.Remove(entity);
            await nodeContext.SaveChangesAsync();

            var invoices = new Invoices(nodeContext);
            await invoices.CancelPending(userId);

            var subjects = new Subjects(nodeContext, subjectCode);
            await subjects.Rebuild();

            return InvoiceWorkflowActionResult.Success("Invoice cancelled.");
        }

        public async Task<InvoiceSubmitModel> GetSubmitAsync(string invoiceNumber, string? emailAddress = null)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();
            var fileProvider = scope.ServiceProvider.GetRequiredService<IFileProvider>();
            var nodeSettings = new NodeSettings(nodeContext);

            var header = await nodeContext.Invoice_Register
                .AsNoTracking()
                .FirstAsync(invoice => invoice.InvoiceNumber == invoiceNumber);

            await EnsureInvoiceSubmissionAuthorisedAsync(nodeContext, header.UserId);

            var templateOptions = await (from i in nodeContext.Web_tbTemplateInvoices.AsNoTracking()
                                         join t in nodeContext.Web_tbTemplates.AsNoTracking() on i.TemplateId equals t.TemplateId
                                         where i.InvoiceTypeCode == header.InvoiceTypeCode
                                         orderby i.LastUsedOn descending
                                         select new InvoiceSubmitTemplateOption
                                         {
                                             TemplateId = t.TemplateId,
                                             TemplateFileName = t.TemplateFileName,
                                             LastUsedOn = i.LastUsedOn
                                         })
                .ToListAsync();

            var recipients = await nodeContext.Subject_EmailAddresses
                .AsNoTracking()
                .Where(item => item.SubjectCode == header.SubjectCode)
                .OrderBy(item => item.EmailAddress)
                .Select(item => new InvoiceSubmitRecipientOption
                {
                    EmailAddress = item.EmailAddress,
                    DisplayName = string.IsNullOrWhiteSpace(item.ContactName)
                        ? item.EmailAddress
                        : $"{item.ContactName} <{item.EmailAddress}>",
                    IsAdmin = item.IsAdmin
                })
                .ToListAsync();

            var selectedTemplate = templateOptions.FirstOrDefault()?.TemplateFileName ?? string.Empty;
            var selectedEmail = ResolveDefaultEmailAddress(recipients, emailAddress);

            var readiness = new InvoiceSubmitReadinessModel
            {
                HasMailHost = nodeSettings.HasMailHost,
                HasTemplates = templateOptions.Count > 0,
                HasRecipients = recipients.Count > 0
            };

            if (!readiness.HasMailHost)
                readiness.Messages.Add("Mail host is not configured.");

            if (!readiness.HasTemplates)
                readiness.Messages.Add("No template is assigned to this invoice type.");

            if (!readiness.HasRecipients)
                readiness.Messages.Add("No email address is available for this subject.");

            return new InvoiceSubmitModel
            {
                InvoiceNumber = header.InvoiceNumber,
                SubjectCode = header.SubjectCode,
                ParentSubjectCode = header.ParentSubjectCode ?? string.Empty,
                NamespacePath = BuildNamespacePath(header.ParentSubjectCode, header.SubjectCode),
                SubjectBrowserUrl = BuildSubjectBrowserUrl(header.ParentSubjectCode, header.SubjectCode),
                SubjectName = header.SubjectName,
                InvoiceType = header.InvoiceType,
                InvoiceTypeCode = header.InvoiceTypeCode,
                InvoicedOn = header.InvoicedOn,
                DueOn = header.DueOn,
                InvoiceValue = (decimal)header.InvoiceValue,
                TaxValue = (decimal)header.TaxValue,
                TotalValue = (decimal)header.TotalInvoiceValue,
                Printed = header.Printed,
                SelectedTemplateFileName = selectedTemplate,
                SelectedEmailAddress = selectedEmail,
                TemplateOptions = templateOptions,
                RecipientOptions = recipients,
                Readiness = readiness
            };
        }

        public async Task<InvoiceSubmitPreviewModel> GetSubmitPreviewAsync(InvoiceSubmitModel model)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();
            var fileProvider = scope.ServiceProvider.GetRequiredService<IFileProvider>();
            var templateManager = new TemplateManager(nodeContext, fileProvider);

            var invoice = await nodeContext.Invoice_tbInvoices
                .AsNoTracking()
                .SingleAsync(item => item.InvoiceNumber == model.InvoiceNumber);

            await EnsureInvoiceSubmissionAuthorisedAsync(nodeContext, invoice.UserId);

            var templateId = await nodeContext.Web_tbTemplates
                .AsNoTracking()
                .Where(item => item.TemplateFileName == model.SelectedTemplateFileName)
                .Select(item => item.TemplateId)
                .FirstAsync();

            MailDocument doc = await templateManager.GetInvoice((NodeEnum.InvoiceType)invoice.InvoiceTypeCode, templateId);
            MailInvoice mailInvoice = new(nodeContext, doc, model.InvoiceNumber);

            var htmlBody = await mailInvoice.PreviewInvoice();
            await templateManager.RegisterTemplateUsage(templateId, (NodeEnum.InvoiceType)invoice.InvoiceTypeCode);

            return new InvoiceSubmitPreviewModel
            {
                InvoiceNumber = model.InvoiceNumber,
                SubjectName = model.SubjectName,
                InvoiceType = model.InvoiceType,
                TemplateFileName = model.SelectedTemplateFileName,
                EmailAddress = model.SelectedEmailAddress,
                HtmlBody = htmlBody,
                Printed = model.Printed
            };
        }

        public async Task<InvoiceWorkflowActionResult> SendSubmitAsync(InvoiceSubmitModel model)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();
            var fileProvider = scope.ServiceProvider.GetRequiredService<IFileProvider>();
            var templateManager = new TemplateManager(nodeContext, fileProvider);

            if (string.IsNullOrWhiteSpace(model.SelectedTemplateFileName))
                return InvoiceWorkflowActionResult.Failure("Please select a template.");

            if (string.IsNullOrWhiteSpace(model.SelectedEmailAddress))
                return InvoiceWorkflowActionResult.Failure("Please select a recipient.");

            var invoice = await nodeContext.Invoice_tbInvoices
                .AsNoTracking()
                .SingleAsync(item => item.InvoiceNumber == model.InvoiceNumber);

            await EnsureInvoiceSubmissionAuthorisedAsync(nodeContext, invoice.UserId);

            var templateId = await nodeContext.Web_tbTemplates
                .AsNoTracking()
                .Where(item => item.TemplateFileName == model.SelectedTemplateFileName)
                .Select(item => item.TemplateId)
                .FirstAsync();

            MailDocument doc = await templateManager.GetInvoice((NodeEnum.InvoiceType)invoice.InvoiceTypeCode, templateId);
            MailInvoice mailInvoice = new(nodeContext, doc, model.InvoiceNumber);

            await mailInvoice.Send(model.SelectedEmailAddress);
            await templateManager.RegisterTemplateUsage(templateId, (NodeEnum.InvoiceType)invoice.InvoiceTypeCode);

            return InvoiceWorkflowActionResult.Success("Invoice submitted.");
        }

        public async Task<InvoiceWorkflowActionResult> MarkInvoiceAsSentAsync(string invoiceNumber)
        {
            using var scope = _scopeFactory.CreateScope();
            var nodeContext = scope.ServiceProvider.GetRequiredService<NodeContext>();

            var invoice = await nodeContext.Invoice_tbInvoices
                .FirstOrDefaultAsync(item => item.InvoiceNumber == invoiceNumber);

            if (invoice is null)
                return InvoiceWorkflowActionResult.Failure("Invoice not found.");

            await EnsureInvoiceSubmissionAuthorisedAsync(nodeContext, invoice.UserId);

            invoice.Spooled = false;
            invoice.Printed = true;

            await nodeContext.SaveChangesAsync();

            return InvoiceWorkflowActionResult.Success("Invoice marked as sent.");
        }

        private static void ApplyNamespaceSelection(InvoiceRaiseEditModel model)
        {
            var namespacePath = model.NamespacePath?.Trim().Trim('.') ?? string.Empty;

            if (string.IsNullOrWhiteSpace(namespacePath))
                return;

            var segments = namespacePath
                .Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (segments.Length == 0)
                return;

            model.SubjectCode = segments[^1];
            model.ParentSubjectCode = segments.Length > 1
                ? segments[^2]
                : string.Empty;
            model.NamespacePath = BuildNamespacePath(model.ParentSubjectCode, model.SubjectCode);
        }

        private static async Task<string> GetDefaultParentSubjectCodeAsync(NodeContext nodeContext, string? subjectCode)
        {
            if (string.IsNullOrWhiteSpace(subjectCode))
                return string.Empty;

            return await nodeContext.Subject_tbNamespaces
                .AsNoTracking()
                .Where(item => item.ChildSubjectCode == subjectCode)
                .OrderByDescending(item => item.IsDefault)
                .ThenBy(item => item.ParentSubjectCode)
                .Select(item => item.ParentSubjectCode)
                .FirstOrDefaultAsync() ?? string.Empty;
        }

        private static string BuildNamespacePath(string? parentSubjectCode, string? subjectCode)
        {
            if (string.IsNullOrWhiteSpace(subjectCode))
                return string.Empty;

            return string.IsNullOrWhiteSpace(parentSubjectCode)
                ? subjectCode
                : $"{parentSubjectCode}.{subjectCode}";
        }

        private static string BuildSubjectBrowserUrl(string? parentSubjectCode, string? subjectCode)
        {
            var namespacePath = BuildNamespacePath(parentSubjectCode, subjectCode);

            if (string.IsNullOrWhiteSpace(namespacePath))
                return "/Subject/Browser/Index";

            return $"/Subject/Browser/Index?mode=Namespace&select={Uri.EscapeDataString(namespacePath)}&namespaceFilter={Uri.EscapeDataString(namespacePath)}";
        }

        private static NodeEnum.CashPolarity ResolveCashPolarity(NodeEnum.InvoiceType invoiceType)
        {
            return invoiceType switch
            {
                NodeEnum.InvoiceType.SalesInvoice => NodeEnum.CashPolarity.Income,
                NodeEnum.InvoiceType.CreditNote => NodeEnum.CashPolarity.Income,
                NodeEnum.InvoiceType.PurchaseInvoice => NodeEnum.CashPolarity.Expense,
                NodeEnum.InvoiceType.DebitNote => NodeEnum.CashPolarity.Expense,
                _ => NodeEnum.CashPolarity.Neutral
            };
        }

        private static string BuildPostSuccessMessage(short invoiceTypeCode)
        {
            return invoiceTypeCode == (short)NodeEnum.InvoiceType.SalesInvoice
                || invoiceTypeCode == (short)NodeEnum.InvoiceType.CreditNote
                ? "Entry posted. The invoice remains available for submission."
                : "Entry posted. The invoice is marked as sent by default.";
        }

        private async Task EnsureInvoiceSubmissionAuthorisedAsync(NodeContext nodeContext, string invoiceUserId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var user = httpContext?.User;

            if (user is null || !(user.Identity?.IsAuthenticated ?? false))
                throw new UnauthorizedAccessException("User is not authenticated.");

            if (user.IsInRole("Managers") || user.IsInRole("Administrators"))
                return;

            var profile = new Profile(nodeContext);
            var externalUserId = user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                ?? user.FindFirst("sub")?.Value
                ?? string.Empty;

            var internalUserId = await profile.UserId(externalUserId);

            if (!string.Equals(internalUserId, invoiceUserId, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("User is not authorised to submit this invoice.");
        }

        private static async Task<InvoiceRaiseEntrySummaryModel> ToSummaryAsync(NodeContext nodeContext, Invoice_vwEntry header)
        {
            var parentSubjectCode = string.IsNullOrWhiteSpace(header.ParentSubjectCode)
                ? await GetDefaultParentSubjectCodeAsync(nodeContext, header.SubjectCode)
                : header.ParentSubjectCode;

            return new InvoiceRaiseEntrySummaryModel
            {
                EntryId = header.EntryId,
                InvoiceNumberOrAccountDisplay = header.SubjectCode,
                SubjectCode = header.SubjectCode,
                ParentSubjectCode = parentSubjectCode,
                NamespacePath = BuildNamespacePath(parentSubjectCode, header.SubjectCode),
                SubjectName = header.SubjectName,
                CashCode = header.CashCode,
                CashDescription = header.CashDescription,
                TaxCode = header.TaxCode,
                TaxDescription = header.TaxDescription,
                InvoiceTypeCode = header.InvoiceTypeCode,
                InvoiceType = header.InvoiceType,
                InvoicedOn = header.InvoicedOn,
                TotalValue = header.TotalValue,
                InvoiceValue = header.InvoiceValue,
                ItemReference = header.ItemReference
            };
        }

        private static async Task<IReadOnlyList<InvoiceRegisterSelectOption>> GetCashCodeOptionsAsync(NodeContext nodeContext)
        {
            return await nodeContext.Cash_CodeLookup
                .AsNoTracking()
                .Where(code => code.CashTypeCode < (short)NodeEnum.CashType.Money)
                .OrderBy(code => code.CashDescription)
                .Select(code => new InvoiceRegisterSelectOption(code.CashCode, code.CashDescription))
                .ToListAsync();
        }

        private static async Task<IReadOnlyList<InvoiceRegisterSelectOption>> GetCashCodeOptionsByPolarityAsync(NodeContext nodeContext, NodeEnum.CashPolarity polarity)
        {
            return await nodeContext.Cash_CodeLookup
                .AsNoTracking()
                .Where(code => code.CashTypeCode < (short)NodeEnum.CashType.Money && code.CashPolarityCode == (short)polarity)
                .OrderBy(code => code.CashDescription)
                .Select(code => new InvoiceRegisterSelectOption(code.CashCode, code.CashDescription))
                .ToListAsync();
        }

        private static async Task<IReadOnlyList<InvoiceRegisterSelectOption>> GetTaxCodeOptionsAsync(NodeContext nodeContext)
        {
            return await nodeContext.App_TaxCodes
                .AsNoTracking()
                .OrderBy(tax => tax.TaxDescription)
                .Select(tax => new InvoiceRegisterSelectOption(tax.TaxCode, tax.TaxDescription))
                .ToListAsync();
        }

        private static async Task<IReadOnlyList<InvoiceRegisterInvoiceTypeOption>> GetInvoiceTypeOptionsAsync(NodeContext nodeContext)
        {
            return await nodeContext.Invoice_tbTypes
                .AsNoTracking()
                .OrderBy(type => type.InvoiceTypeCode)
                .Select(type => new InvoiceRegisterInvoiceTypeOption(type.InvoiceTypeCode, type.InvoiceType))
                .ToListAsync();
        }

        private static async Task<IReadOnlyList<InvoiceRegisterInvoiceStatusOption>> GetInvoiceStatusOptionsAsync(NodeContext nodeContext)
        {
            return await nodeContext.Invoice_tbStatuses
                .AsNoTracking()
                .OrderBy(status => status.InvoiceStatusCode)
                .Select(status => new InvoiceRegisterInvoiceStatusOption(status.InvoiceStatusCode, status.InvoiceStatus))
                .ToListAsync();
        }

        private static string ResolveDefaultEmailAddress(IReadOnlyList<InvoiceSubmitRecipientOption> recipients, string? requestedEmailAddress)
        {
            if (recipients.Count == 0)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(requestedEmailAddress))
            {
                var requested = recipients.FirstOrDefault(item =>
                    string.Equals(item.EmailAddress, requestedEmailAddress, StringComparison.OrdinalIgnoreCase));

                if (requested is not null)
                    return requested.EmailAddress;
            }

            var admin = recipients.FirstOrDefault(item => item.IsAdmin);
            if (admin is not null)
                return admin.EmailAddress;

            return recipients[0].EmailAddress;
        }
    }
}
