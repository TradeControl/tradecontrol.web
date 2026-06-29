# Invoice Register Specification — Version 3

**Draft — 26 June 2026**  
**Trade Control Web**

## 1. Purpose

The Invoice Register is the unified module for viewing, creating, editing, inspecting, and submitting invoices within Trade Control.  
It replaces the fragmented Razor Page implementation under `Pages/Invoice` with a modern, coherent Blazor architecture consistent with the Subject Browser and Admin Manager modules.

The module provides a single, predictable surface for all invoice‑related tasks in Accounts Mode.

## 2. Scope

The Invoice Register delivers five core capabilities:

### 2.1 Register

A period‑based overview of invoices sent and received, with filters, totals, and navigation into detail.

### 2.2 Raise

A workflow for creating new invoices by Cash Code (Accounts Mode miscellaneous invoicing).

### 2.3 Edit

A workflow for modifying existing invoices, including header fields, line items, and posting actions.

### 2.4 Enquiry

A full inspection surface showing every field of the selected invoice, including history and cash‑code breakdown.

### 2.5 Submission

A preview/send workflow using HTML templates and the configured mail host.

These capabilities correspond directly to the functional behaviour of the existing prototype.

## 3. Architecture Pattern

The Invoice Register follows the established Trade Control Blazor pattern used by Subject Browser and Admin Manager:

### 3.1 Hybrid Entry Point  

    Pages/Invoices/Register/Index.cshtml

A Razor Page providing routing, authentication, and hybrid hosting of the Blazor shell.

### 3.2 Shell Component  

    Pages/Invoices/Register/RegisterShell.razor

The root of the Blazor component tree.  
Responsible for:

- loading initial state  
- orchestrating filters  
- coordinating tab navigation  
- invoking the service layer  
- managing UI state and transitions  

### 3.3 Component Set  

    Pages/Invoices/Register/Components/

Localised UI surfaces:

- HeaderList  
- DetailList  
- CashCodeList  
- SummaryBar  
- StatusBadge  
- TypeBadge  
- Edit panels  
- Enquiry panels  
- Submission panel  

Each component is self‑contained, with no deep inheritance and no cross‑component coupling.

### 3.4 Service Layer  

    AppServices\IInvoiceRegisterService.cs  
    AppServices\InvoiceRegisterService.cs  
    AppServices\InvoiceRegisterQueryBuilder.cs  
    AppServices\InvoiceFormattingService.cs

Responsibilities:

- query headers, details, and cash‑code aggregates  
- apply filters, sorting, and paging  
- perform invoice creation and editing  
- execute posting and rebuild logic  
- generate email documents and send via MailService  
- provide formatting metadata (status/type colours, overdue flags)  

### 3.5 UI‑Binding Models  

    Pages/Invoices/Register/Models/

Models for:

- filter state  
- service results  
- header editing  
- item editing  

These models are not EF entities and contain no business logic.

### 3.6 Stylesheet  

    wwwroot/css/modules/invoiceRegister.css

Defines layout, spacing, and theme‑aware presentation.

## 4. Data Surfaces

### 4.1 Header View

- Invoice number  
- Date  
- Subject  
- Invoice type  
- Status  
- Outstanding amount  
- Total value  
- Overdue indicator  
- Type/status badges  

### 4.2 Detail View

- Line items  
- Cash code  
- Tax code  
- Item reference  
- Line totals  

### 4.3 Cash Code View

- Aggregated totals by cash code  
- Count of invoices  
- Outstanding totals  

### 4.4 Enquiry View

- All header fields  
- All line items  
- Cash‑code breakdown  
- Change log  
- Printed/email flags  
- Rebuild indicators  

### 4.5 Edit View

- Editable header fields  
- Editable line items  
- Recalculated totals  
- Validation messages  
- Posting actions  

### 4.6 Submission View

- Template selection  
- Recipient selection  
- HTML preview  
- Send action  
- Printed flag update  

## 5. Workflows

### 5.1 Register Workflow

**Entry:** Module load  
**Actions:**  

- adjust filters  
- navigate tabs  
- open invoice in Enquiry or Edit  

**Completion:**  

- filtered, sorted, paged view with totals  

### 5.2 Raise Workflow

**Entry:** “New Invoice”  
**Actions:**  

- select subject  
- add line items  
- assign cash/tax codes  
- save draft or post  

**Completion:**  

- invoice created and visible in Register  

### 5.3 Edit Workflow

**Entry:** selecting an invoice from Register  
**Actions:**  

- modify header  
- modify items  
- recalc totals  
- detect rebuilds  
- post invoice  

**Completion:**  

- invoice updated and validated  

### 5.4 Enquiry Workflow

**Entry:** selecting an invoice from Register  
**Actions:**  

- inspect all fields  
- view history  
- view cash‑code breakdown  

**Completion:**  

- user returns to Register  

### 5.5 Submission Workflow

**Entry:** selecting “Send”  
**Actions:**  

- choose template  
- choose recipient  
- preview  
- send email  

**Completion:**  

- email sent  
- printed flag updated  

## 6. Navigation Model

### 6.1 High‑Level Flow  

    Filters → Register → (Enquiry | Edit | Submission)

### 6.2 Back Navigation

Back always returns to the previous context:

- Enquiry → Register  
- Edit → Register  
- Submission → Register  

### 6.3 Tabs

- Header  
- Details  
- Cash Code  

Tabs reflect the same invoice set and filter state.

## 7. Service Contracts

### 7.1 Query

`InvoiceRegisterResult Query(InvoiceFilterModel filter)`

Returns:

- headers  
- details  
- cash‑code aggregates  
- paging metadata  
- summary totals  

### 7.2 Editing

`InvoiceEditResult Edit(InvoiceEditModel model)`  
`InvoiceItemEditResult EditItem(InvoiceItemEditModel model)`

### 7.3 Posting

`PostResult Post(invoiceNumber)`

### 7.4 Email

`EmailPreviewResult GenerateDocument(invoiceNumber, template)`  
`EmailSendResult Send(invoiceNumber, template, recipient)`

### 7.5 Formatting

`FormattingMetadata GetFormatting(invoiceNumber)`

## 8. UI Contracts

Each component receives:

- input parameters (data, state, callbacks)  
- emits events (selection, edit, submit)  
- never accesses NodeContext directly  
- never performs business logic  

The Shell coordinates all interactions.

## 9. Non‑Goals

The Invoice Register does **not**:

- redesign invoice lifecycle rules  
- modify SQL schema  
- change posting logic  
- introduce new invoice types  
- implement Projects Mode  
- replace MailService  
- replace Namespace Browser  

## 10. Future Extensions

- Project‑driven invoicing  
- Multi‑invoice submission  
- Bulk posting  
- Saved filter sets  

# 11. Prototype Behaviour (Authoritative Baseline)

The existing prototype under `Pages/Invoice/*` is the **functional reference implementation**.  
It demonstrates:

### 11.1 Full Feature Coverage

The prototype implements all five capabilities:

- Register  
- Raise  
- Edit  
- Enquiry  
- Submission  

### 11.2 Real Integrations

The prototype uses:

- real SQL views (`Invoice_vwRegister`, `Invoice_vwRegisterDetail`, etc.)  
- real stored procedures  
- real invoice lifecycle (`Invoices.cs`)  
- real email templates (`wwwroot/content/templates`)  
- real MailService  
- real Subject DAG  
- real Cash Code logic  
- real period logic  
- real authorisation rules  

### 11.3 Behaviour to Preserve

- filter semantics  
- paging and sorting  
- invoice creation rules  
- editing and posting rules  
- rebuild detection  
- email workflow  
- printed flag behaviour  
- outstanding/overdue logic  

### 11.4 Behaviour to Replace

- UI layout  
- navigation model  
- component structure  
- styling  
- Razor Page fragmentation  
- per‑page data loading  
- ad‑hoc filtering logic  

### 11.5 Aider File Adds

The prototype file list corresponds to the new module’s functional baseline.  
The new module will replace:

    Pages/Invoice/Enquiry/*
    Pages/Invoice/Raise/*
    Pages/Invoice/Update/*

with:

    Pages/Invoices/Register/*
    Pages/Invoices/Register/Components/*
    AppServices/IInvoiceRegisterService.cs
    AppServices/InvoiceRegisterService.cs
    AppServices/InvoiceRegisterQueryBuilder.cs
    AppServices/InvoiceFormattingService.cs


## Appendix A — Integration Context (Non‑Authoritative)

### A.1 Repo Placement

The module sits alongside:

- Subject Browser  
- Admin Manager  
- Cash Manager  
- Tax Configurator  

### A.2 Dependencies

- AppServices  
- Data (Invoices, FinancialPeriods, Subjects)  
- Mail  
- Models (Invoice_vwRegister*, Invoice_tbInvoice, etc.)  

### A.3 Legacy Pages Retired

All Razor Pages under `Pages/Invoice` except MudRazorEval will be removed.

## Appendix B - Aider Files

### Razor Pages

    /add src/TCWeb/Pages/Invoice/Enquiry/Details.cshtml  
    /add src/TCWeb/Pages/Invoice/Enquiry/Details.cshtml.cs  
    /add src/TCWeb/Pages/Invoice/Enquiry/Index.cshtml  
    /add src/TCWeb/Pages/Invoice/Enquiry/Index.cshtml.cs  
    /add src/TCWeb/Pages/Invoice/Update/Edit.cshtml  
    /add src/TCWeb/Pages/Invoice/Update/Edit.cshtml.cs  
    /add src/TCWeb/Pages/Invoice/Update/EditItem.cshtml  
    /add src/TCWeb/Pages/Invoice/Update/EditItem.cshtml.cs  
    /add src/TCWeb/Pages/Invoice/Enquiry/Summary.cshtml  
    /add src/TCWeb/Pages/Invoice/Enquiry/Summary.cshtml.cs  
    /add src/TCWeb/Pages/Invoice/Enquiry/Unpaid.cshtml  
    /add src/TCWeb/Pages/Invoice/Enquiry/Unpaid.cshtml.cs  
    /add src/TCWeb/Pages/Invoice/Enquiry/UnpaidDetail.cshtml  
    /add src/TCWeb/Pages/Invoice/Enquiry/UnpaidDetail.cshtml.cs  
    /add src/TCWeb/Pages/Invoice/Raise/Create.cshtml  
    /add src/TCWeb/Pages/Invoice/Raise/Create.cshtml.cs  
    /add src/TCWeb/Pages/Invoice/Raise/Delete.cshtml  
    /add src/TCWeb/Pages/Invoice/Raise/Delete.cshtml.cs  
    /add src/TCWeb/Pages/Invoice/Raise/Details.cshtml  
    /add src/TCWeb/Pages/Invoice/Raise/Details.cshtml.cs  
    /add src/TCWeb/Pages/Invoice/Raise/Edit.cshtml  
    /add src/TCWeb/Pages/Invoice/Raise/Edit.cshtml.cs  
    /add src/TCWeb/Pages/Invoice/Raise/Index.cshtml  
    /add src/TCWeb/Pages/Invoice/Raise/Index.cshtml.cs  
    /add src/TCWeb/Pages/Invoice/Raise/Post.cshtml  
    /add src/TCWeb/Pages/Invoice/Raise/Post.cshtml.cs  
    /add src/TCWeb/Pages/Invoice/Update/CreateItem.cshtml  
    /add src/TCWeb/Pages/Invoice/Update/CreateItem.cshtml.cs  
    /add src/TCWeb/Pages/Invoice/Update/Delete.cshtml  
    /add src/TCWeb/Pages/Invoice/Update/Delete.cshtml.cs  
    /add src/TCWeb/Pages/Invoice/Update/DeleteItem.cshtml  
    /add src/TCWeb/Pages/Invoice/Update/DeleteItem.cshtml.cs  
    /add src/TCWeb/Pages/Invoice/Update/EmailConfirm.cshtml  
    /add src/TCWeb/Pages/Invoice/Update/EmailConfirm.cshtml.cs  
    /add src/TCWeb/Pages/Invoice/Update/EmailPreview.cshtml  
    /add src/TCWeb/Pages/Invoice/Update/EmailPreview.cshtml.cs  
    /add src/TCWeb/Pages/Invoice/Update/Index.cshtml  
    /add src/TCWeb/Pages/Invoice/Update/Index.cshtml.cs  

### Page Base

    /add Pages/DI_BasePageModel.cs  
    /add Data/NodeSettings.cs  
    /add Data/NodeAdmin.cs  
    /add Data/Docs.cs  

### Data Layer

    /add Data/NodeContext.cs  
    /add Data/NodeContext.Conventions.cs  
    /add Data/NodeContext.Triggers.cs  
    /add Data/NodeContextProc.cs  
    /add Data/Invoices.cs
    /add Data/FinancialPeriods.cs

### Models

    /add Models/Invoice_tbInvoice.cs  
    /add Models/Invoice_tbItem.cs  
    /add Models/Invoice_tbStatus.cs  
    /add Models/Invoice_tbType.cs  
    /add Models/Invoice_tbEntry.cs  
    /add Models/Invoice_vwChangeLog.cs  
    /add Models/Invoice_vwEntry.cs  
    /add Models/Invoice_vwRegister.cs  
    /add Models/Invoice_vwRegisterDetail.cs  
    /add Models/Invoice_vwRegisterItem.cs  
    /add Models/Invoice_vwRegisterCashCode.cs  
    /add Models/Invoice_vwRegisterExpense.cs  
    /add Models/Invoice_vwRegisterSale.cs  
    /add Models/Invoice_vwRegisterPurchase.cs  
    /add Models/Invoice_vwRegisterOverdue.cs  

**Supporting Models**

    /add Models/App_tbTaxCode.cs  
    /add Models/App_tbTaxTag.cs  
    /add Models/App_tbTaxTagMap.cs  
    /add Models/App_tbTaxTagSource.cs  
    /add Models/App_vwTaxCode.cs  
    /add Models/Cash_tbCode.cs  
    /add Models/Cash_vwCodeLookup.cs  
    /add Models/Usr_tbUser.cs  
    /add Models/App_tbPeriod.cs  
    /add Models/App_vwActivePeriod.cs  

### Subjects

    /add Data/Subjects.cs  
    /add Models/Subject_tbSubject.cs  
    /add Models/Subject_vwSubjectLookup.cs
    /add Models/Subject_vwInvoiceSummary.cs  
    /add src/TCWeb/Pages/Subject/Controls/NamespaceSelector.razor  
    /add src/TCWeb/Pages/Subject/Controls/NamespaceSelectorSuggestion.cs  

### Enums

### Identity

    /add Data/Profile.cs  
    /add Areas/Identity/Data/TradeControlWebUser.cs  
    /add Authorization/AspNetAuthorizationHandler.cs  
    /add Authorization/Operations.cs  

### Mail

    /add Mail/MailInvoice.cs  
    /add Mail/MailService.cs  
    /add Mail/TemplateManager.cs  
    /add Models/Web_tbTemplate.cs  

### Stylesheets

    /add src/TCWeb/wwwroot/css/base.css  
    /add src/TCWeb/wwwroot/css/modules/invoiceRegister.css
    /add src/TCWeb/wwwroot/css/modules/subjectBrowser.css
    /add src/TCWeb/wwwroot/css/themes/theme-blue.css
    /add src/TCWeb/wwwroot/css/themes/theme-dark.css
    /add src/TCWeb/wwwroot/css/themes/theme-green.css
    /add src/TCWeb/wwwroot/css/themes/theme-orange.css
    /add src/TCWeb/wwwroot/css/themes/theme-pink.css
    /add src/TCWeb/wwwroot/css/themes/theme-red.css

** END of v3 **
