# Invoice Register Specification — Version 2

23 June 2026  

## 1. Overview

The Invoice Register is the consolidated module for viewing, filtering, creating, editing, posting, and analysing invoices within Trade Control. It replaces the legacy Razor Pages under /Invoice/Enquiry, /Invoice/Raise, and /Invoice/Update, bringing all functionality into a unified Blazor architecture.

The module provides:

- A two‑pane desktop layout with filters on the left and a tabbed ledger on the right
- A mobile layout with a collapsible filter drawer and full‑screen tab views
- A unified query model for headers, details, and cash‑code aggregates
- A unified workflow for creating and editing invoices
- A unified workflow for email generation and sending
- A future‑ready structure for Project‑driven invoicing

This specification defines the structure, behaviour, and UI surfaces for the minimal Accounts Mode release.

``` text
AI:
  Overview
    Purpose: Unified invoice management module
    Replaces
      - Invoice.Enquiry
      - Invoice.Raise
      - Invoice.Update
    Capabilities
      - Filtering
      - Sorting
      - Paging
      - Header editing
      - Item editing
      - Posting
      - Email workflow
    Layout
      Desktop: TwoPane
      Mobile: Drawer + FullScreenTabs
    Modes
      - AccountsMode (current)
      - ProjectsMode (future)
```

## 2. Project Structure

### 2.1 AppServices

AppServices
  IInvoiceRegisterService.cs
  InvoiceRegisterService.cs
  InvoiceRegisterQueryBuilder.cs
  InvoiceFormattingService.cs

Responsibilities:

- Query invoice headers, details, and cash‑code aggregates
- Apply filters, sorting, and paging
- Create invoices and manage draft editing
- Perform posting and acceptance logic
- Trigger period and subject rebuilds
- Generate email documents and send via Mail layer
- Provide formatting metadata (status/type colours, overdue flags)

``` text
AI:
  AppServices
    Files
      - IInvoiceRegisterService
      - InvoiceRegisterService
      - InvoiceRegisterQueryBuilder
      - InvoiceFormattingService
    Responsibilities
      Query: Headers, Details, CashCodes
      Filtering: Apply all filter models
      Sorting: ServerSide
      Paging: ServerSide
      Editing: Header + Items
      Posting: Execute domain rules
      RebuildDetection: Period + Subject
      Email: Generate + Send
      Formatting: StatusColours + TypeColours + OverdueFlags
```

### 2.2 Data Layer

The Data layer contains the business logic for invoices. These classes sit between AppServices and NodeContext and implement the domain rules of Trade Control.

Existing classes:

- Invoices.cs — full invoice lifecycle logic (Raise, Credit, Accept, Post, Recalculate, Mirror, CancelPending, etc.)
- NodeEnum.cs — domain enumerations (InvoiceType, InvoiceStatus, DocType, CashPolarity, etc.)
- NodeContext.cs — EF context exposing tables, views
- NodeContextProc.cs - stored procedures

Characteristics:

- Encapsulate business rules
- Call stored procedures via NodeContext
- Perform validation and rebuild detection
- Contain no UI logic
- Do not map directly to SQL tables (that is the role of Models)

``` text
AI:
  DataLayer
    Classes
      - Invoices: DomainLogic
      - NodeEnum: Enumerations
      - NodeContext: EFContext
    Rules
      - No UI logic
      - No direct SQL mapping
      - All lifecycle operations implemented here
      - Rebuild detection centralised
      - Stored procedures invoked via NodeContext
```

### 2.3 Models (EF‑Mapped SQL Classes)

Models represent SQL tables and views. They are pure data containers used by Entity Framework and contain no business logic.

Existing models:

- see Models namespace in Section 5. Files

Characteristics:

- Map directly to SQL schema via EF attributes
- Used by NodeContext to materialise data
- Not used for UI binding
- Not used for business logic

``` text
AI:
  Models
    Purpose: SQLMappingOnly
    Entities
      - see Models in Section 5. Files
    Rules
      - No business logic
      - No UI logic
      - EF attributes define mapping
      - Used only by NodeContext
```

### 2.4 UI‑Binding Models (New)

These models are not EF entities. They are used only for UI state, filter binding, and service results.

They belong under:

Pages/Invoice/Register/Models

InvoiceFilterModel:

- PeriodYear
- PeriodMonth
- ShowAll
- InvoiceType
- Namespace
- DateFrom
- DateTo
- Status filters (Draft, Posted, Unsent, Unpaid)
- CashCode
- Paging (PageNumber, PageSize)
- Sorting (SortField, SortDirection)

InvoiceRegisterResult:

- Headers (from Invoice_vwRegister)
- Details (from Invoice_vwRegisterDetail)
- CashCodes (from Invoice_vwRegisterCashCode)
- Paging metadata
- Summary totals

InvoiceEditModel:

- Header editing fields (InvoiceType, InvoiceStatus, InvoicedOn, DueOn, ExpectedOn, PaymentTerms, Notes, Printed flag)
- Validation metadata

InvoiceItemEditModel:

- Line editing fields (TaxCode, TotalValue, InvoiceValue, ItemReference, CashCode)
- Validation metadata

``` text
AI:
  UIBindingModels
    Location: Pages.Invoice.Register.Models
    InvoiceFilterModel
      - PeriodYear
      - PeriodMonth
      - ShowAll
      - InvoiceType
      - Namespace
      - DateFrom
      - DateTo
      - StatusFlags
      - CashCode
      - Paging
      - Sorting
    InvoiceRegisterResult
      - Headers
      - Details
      - CashCodes
      - Paging
      - Totals
    InvoiceEditModel
      - HeaderFields
      - Validation
    InvoiceItemEditModel
      - ItemFields
      - Validation
```

### 2.5 Pages (Blazor)

Pages/Invoice/Register
  RegisterShell.razor
  Filters.razor

Components
  InvoiceHeaderList.razor
  InvoiceDetailList.razor
  InvoiceCashCodeList.razor
  InvoiceSummaryBar.razor
  InvoiceStatusBadge.razor
  InvoiceTypeBadge.razor

Stylesheet
  css/invoiceRegister.css

RegisterShell.razor:

- Two‑pane layout (desktop)
- Drawer‑based filter panel (mobile)
- Tabbed ledger (Header / Details / CashCode)
- Integration with service layer

Filters.razor:

- Year/month selector
- Show All toggle
- InvoiceType
- Namespace
- Date override
- Status filters
- Cash Code
- Future: Project filters

Components:

- Header list (paged, sortable, conditional formatting)
- Detail list (paged, sortable)
- Cash‑code aggregates
- Summary bar
- Status/type badges

``` text
AI:
  Pages
    RegisterShell
      Layout
        Desktop: TwoPane
        Mobile: Drawer
      Tabs
        - Header
        - Details
        - CashCode
      IntegratesWith: InvoiceRegisterService
    Filters
      Fields
        - Period
        - ShowAll
        - InvoiceType
        - Namespace
        - DateOverride
        - StatusFlags
        - CashCode
    Components
      HeaderList: Paged + Sortable + ConditionalFormatting
      DetailList: Paged + Sortable
      CashCodeList: Aggregated
      SummaryBar: VisibleAlways
      Badges: Status + Type

```

## 3. Deprecated Prototype Pages

### 3.1 /Invoice/Enquiry

Preserved behaviour:

- Header/detail listing
- Summary by cash code
- Unpaid invoice view
- Period filtering
- Printed/Unprinted filtering
- Paging
- Sorting

Discarded behaviour:

- Razor Page UI
- Device‑specific column hiding
- Ad‑hoc filtering logic
- Per‑page data loading
- Inline SQL‑like LINQ patterns

New location:

- Header tab
- Details tab
- CashCode tab
- Unified filter panel
- Unified paging/sorting

``` text
AI:
  Deprecated.Enquiry
    Preserve
      - HeaderListing
      - DetailListing
      - CashCodeSummary
      - UnpaidView
      - PeriodFiltering
      - Paging
      - Sorting
    Discard
      - RazorUI
      - AdHocFiltering
      - PerPageLoading
    NewLocation
      - HeaderTab
      - DetailsTab
      - CashCodeTab
      - UnifiedFilters
```

### 3.2 /Invoice/Raise

Preserved behaviour:

- Invoice creation
- Subject selection
- CashCode selection
- TaxCode selection
- Draft invoice creation
- Navigation to pickers

Discarded behaviour:

- Razor Page UI
- Session‑based navigation
- Page‑local validation

New location:

- “New Invoice” workflow in RegisterShell
- Modal or dedicated surface
- Uses InvoiceRegisterService

``` text
AI:
  Deprecated.Raise
    Preserve
      - CreateInvoice
      - SelectSubject
      - SelectCashCode
      - SelectTaxCode
      - DraftCreation
    Discard
      - RazorUI
      - SessionNavigation
      - LocalValidation
    NewLocation
      - RegisterShell.NewInvoiceWorkflow
```

### 3.3 /Invoice/Update

Preserved behaviour:

- Header editing
- Item editing
- Email template selection
- Email preview
- Email sending
- Printed flag
- Period rebuild logic
- Subject rebuild logic
- Posting logic
- Authorization rules

Discarded behaviour:

- Razor Page UI
- Per‑page data loading
- Repeated authorization logic
- Repeated rebuild detection logic
- Repeated lookup loading

New location:

- Edit Invoice surface (header + items)
- Email workflow (modal or tab)
- Centralised rebuild logic in service layer
- Centralised authorization in service layer

``` text
AI:
  Deprecated.Update
    Preserve
      - EditHeader
      - EditItems
      - EmailWorkflow
      - PrintedFlag
      - RebuildLogic
      - PostingLogic
      - Authorization
    Discard
      - RazorUI
      - RepeatedLogic
    NewLocation
      - EditInvoiceSurface
      - EmailSurface
      - ServiceLayer.Rebuild
      - ServiceLayer.Authorization
```

## 4. Functional Specification

### 4.1 Desktop Layout

Left‑Hand Side (Filters):

- Financial year selector
- Month selector
- Show All toggle
- InvoiceType
- Namespace
- Date override
- Status filters
- Cash Code

Right‑Hand Side (Tabs):

Header tab:

- Paged list
- Sortable
- Conditional formatting
- Outstanding amount
- Status/type badges

Details tab:

- Paged list of line items
- Sortable
- CashCode column

Cash Code tab:

- Aggregated totals
- Count of invoices
- Outstanding totals

``` text
AI:
  DesktopLayout
    LHS.Filters
      - Period
      - ShowAll
      - InvoiceType
      - Namespace
      - DateOverride
      - StatusFlags
      - CashCode
    RHS.Tabs
      Header
        - Paged
        - Sortable
        - ConditionalFormatting
      Details
        - Paged
        - Sortable
        - CashCodeColumn
      CashCode
        - AggregatedTotals
```

### 4.2 Mobile Layout

- Filters collapse into drawer
- Tabs become full‑screen pages
- Reduced column set
- Summary bar remains visible

``` text
AI:
  MobileLayout
    Filters: Drawer
    Tabs: FullScreen
    Columns: Reduced
    SummaryBar: Visible
```

### 4.3 Filtering

Period logic:

- Default: current month
- Show All: year + month expanded
- Date override supersedes period

Status logic:

- Draft
- Posted
- Unsent
- Unpaid
- Overdue (derived)

Namespace logic:

- Uses Subject DAG
- Multi‑level selection

Cash Code logic:

- Exact match
- Future: hierarchical selection

``` text
AI:
  Filtering
    Period
      Default: CurrentMonth
      ShowAll: ExpandYear
      Override: DateRange
    Status
      - Draft
      - Posted
      - Unsent
      - Unpaid
      - Overdue
    Namespace
      Source: SubjectDAG
      Mode: MultiLevel
    CashCode
      Match: Exact
```

### 4.4 Sorting

Sortable fields:

- Date
- Invoice No
- Subject
- Amount
- Status

Sorting is server‑side.

``` text
AI:
  Sorting
    Mode: Server
    Fields
      - Date
      - InvoiceNo
      - Subject
      - Amount
      - Status
```

### 4.5 Paging

- PageSize: 10/25/50/100
- PageNumber
- TotalPages
- TotalItems

Paging is server‑side.

``` text
AI:
  Paging
    Mode: Server
    PageSizes
      - 10
      - 25
      - 50
      - 100
```

### 4.6 Conditional Formatting

InvoiceType → colour:

- Sales
- Purchase
- Misc
- Credit Note

Status → colour/icon:

- Draft
- Posted
- Unsent
- Overdue

Overdue detection:

- DueDate < Today
- Outstanding > 0

``` text
AI:
  ConditionalFormatting
    InvoiceTypeColours
      - Sales
      - Purchase
      - Misc
      - CreditNote
    StatusColours
      - Draft
      - Posted
      - Unsent
      - Overdue
    OverdueRule
      DueDate < Today AND Outstanding > 0
```

### 4.7 Invoice Creation (Accounts Mode)

- Create new invoice
- Add/edit/delete line items
- Assign CashCode
- Assign TaxCode
- Save as draft
- Post invoice
- Mark as sent

Future: Project‑driven invoice generation.

``` text
AI:
  InvoiceCreation
    Actions
      - Create
      - EditItems
      - DeleteItems
      - AssignCashCode
      - AssignTaxCode
      - SaveDraft
      - Post
      - MarkSent
```

### 4.8 Invoice Editing

- Edit header fields
- Edit line items
- Recalculate totals
- Detect period rebuild
- Detect org rebuild
- Trigger:
  - Invoices.Accept()
  - FinancialPeriods.Generate()
  - Subjects.Rebuild()

``` text
AI:
  InvoiceEditing
    Header: Editable
    Items: Editable
    Totals: Recalculate
    RebuildDetection
      - Period
      - Subject
    Triggers
      - Accept
      - GeneratePeriods
      - RebuildSubjects
```

### 4.9 Email Workflow

- Select template
- Select recipient
- Preview document
- Send email
- Register template usage
- Mark invoice as printed

``` text
AI:
  EmailWorkflow
    Steps
      - SelectTemplate
      - SelectRecipient
      - Preview
      - Send
      - RegisterUsage
      - MarkPrinted
```

### 4.10 Authorization

Rules preserved from prototype:

- Managers/Admins may edit any invoice
- Standard users may edit only their own invoices
- Email sending restricted to authorized roles
- Posting restricted to authorized roles

Authorization is centralised in InvoiceRegisterService.

``` text
AI:
  Authorization
    Edit
      Managers: AllInvoices
      StandardUsers: OwnInvoices
    Email: Restricted
    Posting: Restricted
    Centralised: InvoiceRegisterService
```

## 5. Files

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

/add Data/NodeEnum.cs  

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

## 6. Development Workflow

``` text
AI:
  DevWorkflow
    Steps
      - Requirements
      - UIBindingModels
      - QueryBuilder
      - ServiceLayer
      - BlazorPages
      - Navigation
      - Testing
      - Deployment
```

## 7. Final Notes

This Version 2 specification provides the structural and behavioural foundation for the Invoice Register.  
Step 5 will refine the SQL layer and Step 6 will produce the final, polished specification.

``` text
AI:
  FinalNotes
    Status: DraftV2Complete
    NextSteps
      - SQLValidation
      - FinalPolish
```

