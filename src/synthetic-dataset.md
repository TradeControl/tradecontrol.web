# Synthetic dataset generator (T-SQL): 2-year Accounts + MIS validation

**Introduction**

Hello.

We have successfully completed both the Admin Manager and the Category Tree. The template system is now fully implemented and provides a complete, compliant starting point for UK businesses. Our next step is to test the schema changes and produce a dataset that proves all financial, operational, and technical calculations are correct.

To achieve this, we are going to create a synthetic dataset in T‑SQL using the Initialisation Templates that have already been created. These templates are aligned with the relevant Making Tax Digital (MTD) structures and install a complete Category Tree, Cash Codes, rollups, and tax defaults.

Although the first public release will operate in Accounts Mode only, the backend already supports full MIS capabilities. We will use these MIS features to generate a two‑year dataset that exercises the entire system. This will make future MIS implementation in ASP.NET Core significantly easier, and it will also provide a foundation for proofs, tutorials, and validation of our schema changes to the prototype version.

Before defining the dataset, I will explain the templates that have been implemented and the assumptions they provide.

For clarity, I will be presenting my instructions in eight sections. You may respond with questions please but do not attempt to code until you have all the required information.

Are you ready to proceed?
 
## 1. Initialisation Templates

The template system is already fully implemented. The model must not attempt to create templates, modify templates, or generate alternative template structures.

Templates are installed through stored procedures that define the complete and authoritative configuration for a business node. These procedures create the Category Tree, rollups, Cash Codes, tax defaults, and all structural elements required for financial and MIS processing.

**Available Template Procedures**

* `App.proc_Template_CO_MICRO_CUR_MIN_2026`  
  Minimal Micro Company Accounts [YEAR]

* `App.proc_Template_CO_MICRO_CUR_STD_2026`  
  Standard Micro Company Accounts [YEAR]

* `App.proc_Template_ST_SOLE_CUR_2026`  
  Sole Trader Accounts [YEAR]

These procedures install:

* the full Category Tree  
* rollups and totals  
* Cash Codes mapped into Categories  
* VAT and Corporation Tax defaults  
* MTD tax types  
* configuration required for MIS and Accounts Mode  

**Shared VAT Logic**

VAT‑off behaviour is implemented in:

* `App.proc_Template_DisableVAT`

Templates call this automatically when `@IsVatRegistered = 0`.

**Template Parameters**

Templates accept the following parameters:

* `@IsVatRegistered`  
* `@TemplateName` (via the Admin Manager registry)  
* financial year start month  
* bank account configuration  
* government account name  
* and other node‑level settings  

These parameters are already implemented and must be used as‑is.

**Important Instruction for the Model**

Do not generate templates.  
Do not generate Category Trees.  
Do not generate Cash Codes.  
Do not generate rollups.

These already exist and must be treated as fixed infrastructure.  
The MIS dataset must be generated on top of the installed template, not instead of it.

## 2. Category Tree & Cash Polarity

The system uses a unified classification model based on Cash Polarity. This replaces traditional double‑entry bookkeeping (DEBC) and ERP order flows with a single, recursive structure that classifies all business activity as inflows or outflows.

The Category Tree installed by each template provides:

* a complete hierarchy of income, costs, assets, liabilities, and tax categories  
* rollups that define how totals are calculated  
* a polarity‑driven structure that ensures all flows reconcile  
* a consistent layout for the Cash Statement, Profit & Loss, and Balance Sheet  

**Cash Polarity**

Cash Polarity treats all flows—money or goods—as directional:

* **Inflow**  
* **Outflow**

This polarity is encoded in the Category Tree and inherited by all Cash Codes.  
It ensures that:

* income and receipts classify as inflows  
* expenses and payments classify as outflows  
* asset increases classify as inflows  
* asset decreases classify as outflows  
* liability increases classify as inflows  
* liability decreases classify as outflows  

This unified model guarantees that all financial and operational activity can be reconciled through the same structure.

**Category Rollups**

Each template installs a complete set of rollups, including:

* Turnover  
* Cost of Sales  
* Gross Profit  
* Overheads  
* Operating Profit  
* Corporation Tax  
* Net Profit  
* Assets  
* Liabilities  
* Equity  

These rollups are used by:

* the Cash Statement  
* the Profit & Loss  
* the Balance Sheet  
* the MIS engine  
* the tax engine  

**Cash Codes**

Cash Codes are mapped into Categories by the templates.  
Each Cash Code has:

* a CategoryCode  
* a CashPolarityCode  
* a TaxCode  
* an enable/disable flag  

The dataset must use these Cash Codes exactly as installed by the template.  
The model must not create new Cash Codes or modify existing ones.

**Why This Matters for the Dataset**

The two‑year dataset must reconcile across:

* Category totals  
* Cash Code postings  
* polarity rules  
* rollups  
* period boundaries  

Because the Category Tree and Cash Polarity model are fixed, the dataset must be generated in a way that respects:

* the polarity of each Cash Code  
* the rollup structure  
* the template’s category layout  
* the tax behaviour encoded in the tree  

This ensures that the MIS engine, the financial engine, and the tax engine all produce consistent results.

## 3. Tax Engine

The system includes a complete tax engine that supports VAT, Corporation Tax, and Making Tax Digital (MTD) requirements. These behaviours are already implemented in the templates and must be treated as fixed. The dataset must exercise these behaviours without redefining them.

**VAT Behaviour**

VAT is controlled by:

* the `TaxCode` assigned to each Cash Code  
* the VAT recurrence logic in the tax engine  
* the VAT control account  
* the VAT‑off procedure (`App.proc_Template_DisableVAT`)  

When `@IsVatRegistered = 1`:

* VAT is calculated on sales and purchases  
* VAT control movements are generated  
* VAT periods accumulate VAT liability  
* VAT liability appears on the Balance Sheet  

When `@IsVatRegistered = 0`:

* VAT categories are disabled  
* VAT Cash Codes are disabled  
* all Cash Codes default to zero‑rated VAT  
* VAT control movements do not occur  

The dataset must include both VAT‑registered and non‑VAT scenarios.

**Corporation Tax (CT)**

Corporation Tax applies only to company templates.  
The CT behaviour is installed by:

* `App.proc_Template_CO_MICRO_CUR_MIN_2026`  
* `App.proc_Template_CO_MICRO_CUR_STD_2026`

CT recurrence:

* calculates CT based on taxable profit  
* posts CT liability to the Balance Sheet  
* posts CT expense to the P&L  
* supports multi‑period behaviour  

Sole Trader templates do not use Corporation Tax.  
The dataset must include at least one CT‑bearing scenario.

**Making Tax Digital (MTD)**

MTD recurrence is installed by all templates and provides:

* quarterly update categories  
* tax type definitions  
* support for future MTD submissions  

The dataset must generate activity that populates MTD categories, even though the first ASP.NET release does not expose MIS.

**Tax Types and Cash Codes**

Each Cash Code has a `TaxCode` that determines:

* whether VAT applies  
* whether CT applies  
* how the transaction is treated in MTD  
* how the transaction appears in the P&L and Balance Sheet  

The dataset must use the existing Cash Codes exactly as installed by the template.

**Why This Matters for the Dataset**

The two‑year dataset must allow the system to demonstrate:

* VAT liability reconciliation  
* CT liability reconciliation  
* correct P&L tax charges  
* correct Balance Sheet tax balances  
* correct MTD category population  
* correct behaviour when VAT is disabled  
* correct behaviour for Sole Trader vs Company  

The tax engine is already implemented.  
The dataset must exercise it, not redefine it.

## 4. MIS Engine

Although the first ASP.NET Core release operates in Accounts Mode only, the backend already includes a complete MIS (Management Information System) engine. This engine supports full operational modelling, including projects, bills of materials, operations, scheduling, and multi‑level flows. The MIS engine is essential for generating a realistic two‑year dataset that exercises the entire system.

The dataset must use the MIS engine to generate operational activity, but the model must not attempt to redesign or replace MIS logic. Instead, it should use the existing stored procedures and schema as the authoritative implementation.

**MIS Components**

The MIS engine includes the following core elements:

* **Projects**  
  Represent work orders, sales orders, purchase orders, and internal jobs.

* **Bills of Materials (BOMs)**  
  Multi‑level structures defined in `Object.tbFlow` that describe how parent items consume child items.

* **Operations**  
  Defined in `Object.tbOp`, representing manufacturing or service steps.

* **Scheduling**  
  Performed by `Project.proc_Schedule`, which calculates dates based on offsets and dependencies.

* **Flows**  
  Represent parent/child project relationships and material or service dependencies.

* **WIP (Work in Progress)**  
  Automatically generated when projects are scheduled and executed.

* **Supplier Chains**  
  Projects linked to suppliers via `SubjectCode`, with lead times and payment terms.

**Relevant Stored Procedures**

The following procedures define MIS behaviour and must be used as‑is:

* `Project.proc_NextCode`  
* `Project.proc_Configure`  
* `Project.proc_AssignToParent`  
* `Project.proc_Schedule`  
* `Project.proc_Copy`  
* `Project.proc_Pay`  
* `Invoice.proc_Raise`  
* `Invoice.proc_Accept`

These procedures:

* create project codes  
* configure project defaults  
* attach child projects to parents  
* schedule operations  
* generate recurring orders  
* raise invoices  
* accept invoices  
* post payments  

The dataset must use these procedures to generate MIS activity.

**MIS and Accounts Mode**

Even though MIS is not visible in the first release, MIS transactions still:

* generate financial postings  
* consume materials  
* create WIP  
* create supplier liabilities  
* create customer receivables  
* generate VAT movements  
* generate CT‑relevant profit  
* affect the Balance Sheet and P&L  

This makes MIS essential for producing a realistic dataset.

**Why This Matters for the Dataset**

The two‑year dataset must include:

* recurring MIS orders  
* multi‑level BOM consumption  
* supplier purchases  
* customer sales  
* delivery operations  
* WIP creation and clearance  
* monthly invoicing  
* monthly payments  
* multi‑period scheduling  

This ensures that:

* operational flows reconcile with financial flows  
* WIP movements reconcile with inventory and cost of sales  
* supplier chains reconcile with liabilities  
* customer chains reconcile with receivables  
* VAT and CT calculations reflect real activity  

The MIS engine is already implemented.  
The dataset must exercise it, not redefine it.

## 5. Financial Engine

The financial engine is responsible for producing the Profit & Loss, Balance Sheet, Cash Statement, and tax calculations. It operates entirely from the Category Tree, Cash Codes, and polarity rules installed by the templates. The dataset must exercise this engine across two full years to demonstrate correct behaviour.

**Core Financial Statements**

The system produces:

* **Profit & Loss**  
  Driven by income and expense categories, including cost of sales and overheads.

* **Balance Sheet**  
  Driven by asset, liability, and equity categories, including VAT and CT balances.

* **Cash Statement**  
  Driven by inflow/outflow polarity and Cash Code postings.

* **Tax Statements**  
  VAT and CT summaries based on tax recurrence logic.

These statements are derived automatically from the Category Tree and Cash Codes.

**Period Boundaries**

The financial engine supports:

* monthly periods  
* quarterly VAT periods  
* annual CT periods  
* multi‑year continuity  

The dataset must include activity in every month across two years to validate:

* opening balances  
* closing balances  
* retained earnings  
* VAT carry‑forward  
* CT carry‑forward  
* cash reconciliation  

**Financial Postings**

Financial postings are generated by:

* MIS transactions (projects, BOMs, operations, WIP)  
* manual invoices  
* manual payments  
* direct CashCode postings  
* VAT recurrence  
* CT recurrence  

The dataset must include both MIS and non‑MIS postings to reflect real business behaviour.

**Accrual and Cash Behaviour**

The system supports:

* accrual‑based income and expenses  
* cash‑based VAT  
* cash‑based payments  
* receivables and payables  
* WIP and inventory movements  

The dataset must include:

* invoices raised but not yet paid  
* payments without invoices (e.g., deposits)  
* supplier invoices  
* customer invoices  
* WIP creation and clearance  
* inventory consumption  

**Why This Matters for the Dataset**

The two‑year dataset must allow the system to demonstrate:

* correct P&L totals  
* correct Balance Sheet balances  
* correct VAT liability  
* correct CT liability  
* correct cash reconciliation  
* correct retained earnings  
* correct multi‑period behaviour  

The financial engine is already implemented.  
The dataset must exercise it, not redefine it.
 
## 6. Dataset Constraints

The dataset must be generated in T‑SQL and must run on top of the installed template without modifying any template structures. The purpose is to create a realistic two‑year business history that exercises all financial, operational, and tax behaviours.

**Time Span**

* The dataset must cover 24 consecutive months.
* Every month must contain activity.
* Period boundaries must be respected for VAT, CT, and financial statements.

**Template Requirements**

* The dataset must use the template exactly as installed.
* No new Cash Codes, Categories, or rollups may be created.
* The model must use the Cash Codes and Category Tree provided by the template.

**Accrual Requirements**

The dataset must model accruals for:

* **customer income** (invoices raised but unpaid)  
* **supplier costs** (purchases committed but unpaid)  
* **VAT liability** (VAT collected on sales, VAT paid on purchases, quarterly settlement)  
* **Corporation Tax liability** (taxable profit accumulated monthly, CT due nine months after year‑end)

These accruals must reflect both earned activity and future cash consequences.  
This allows the system to demonstrate how businesses forecast:

* revenue  
* expenses  
* VAT payments or refunds  
* Corporation Tax due after year‑end  

**MIS Requirements**

The dataset must include:

* recurring MIS orders  
* multi‑level BOM consumption  
* supplier chains  
* customer chains  
* WIP creation and clearance  
* scheduled operations  
* deliveries  
* project dependencies  
* parent/child flows  

MIS activity must generate financial postings that reconcile with the financial engine.

**Non‑MIS Requirements**

The dataset must also include:

* manually raised invoices  
* manually raised payments  
* direct CashCode postings  
* adjustments  
* ad‑hoc income and expenses  

This reflects real business behaviour where not all activity flows through MIS.

**Tax Requirements**

The dataset must include:

* VAT‑registered scenarios  
* non‑VAT scenarios  
* quarterly VAT periods  
* VAT control account movements  
* CT accrual  
* CT payment timing  
* MTD category population  

**Financial Requirements**

The dataset must reconcile across:

* Profit & Loss  
* Balance Sheet  
* Cash Statement  
* VAT liability  
* CT liability  
* retained earnings  
* opening and closing balances  

The dataset must demonstrate correct multi‑period behaviour across all statements.

**Determinism**

The dataset must be:

* deterministic  
* repeatable  
* template‑agnostic  
* valid for any supported template  

## 7. Dataset Purpose

The purpose of the two‑year dataset is to provide a complete, realistic, and technically rigorous demonstration of the system’s financial, operational, and tax capabilities. Two financial years are needed to reconcile the P&L with the Balance Sheet (such that Profit + Tax + Year 1 capital balance = Year 2 balance). The dataset must validate that all schema changes, template structures, and MIS behaviours operate correctly when subjected to real‑world business activity.

**Proof of Correctness**

The dataset must demonstrate that:

* the Category Tree and Cash Polarity model produce correct totals  
* rollups generate accurate P&L, Balance Sheet, and Cash Statement values  
* VAT and Corporation Tax recurrence behave correctly across multiple periods  
* accruals for income, costs, VAT, and CT are calculated and carried forward correctly  
* MIS activity generates correct financial postings  
* non‑MIS activity integrates seamlessly with MIS activity  
* period boundaries and multi‑year continuity are handled correctly  
**Schema Validation**

The dataset must validate:

* the Admin Manager configuration  
* the template installation logic  
* the Category Tree structure  
* Cash Code mappings  
* tax type behaviour  
* MIS project, flow, and operation structures  
* financial statement generation  
* MTD category population  

This ensures that the schema is stable, coherent, and ready for future development.

**Operational Demonstration**

The dataset must show:

* multi‑level BOM consumption  
* supplier chains and customer chains  
* WIP creation and clearance  
* scheduled operations  
* deliveries  
* recurring orders  
* project dependencies  
* parent/child flows  

This demonstrates that the MIS engine is fully functional even before it is exposed in the ASP.NET Core interface.

**Financial Demonstration**

The dataset must show:

* monthly revenue and cost patterns  
* VAT liability accumulation and settlement  
* CT accrual and payment timing  
* receivables and payables  
* cash flow consequences of operational activity  
* retained earnings movement  
* opening and closing balances across two years  

This provides a complete financial picture of a working business.

**Future Development**

The dataset will serve as the foundation for:

* MIS implementation in ASP.NET Core  
* tutorials and documentation  
* automated testing  
* regression testing for future schema changes  
* validation of new templates  
* demonstration of MIS and Accounts Mode integration  

The dataset must therefore be deterministic, repeatable, and template‑agnostic.

**Summary**

The dataset is not simply a test script.  
It is a full operational and financial simulation that proves the correctness, stability, and expressive power of the entire system.

## 8. Reference Procedure: App.proc_DemoBom

The system includes a demonstration procedure named `App.proc_DemoBom`. This procedure provides a minimal example of how MIS flows, BOM structures, and project dependencies can be instantiated within the system. It is not a template, and it must not be copied, modified, or extended directly.

`App.proc_DemoBom` exists solely to illustrate:

* how a Bill of Materials (BOM) is represented using Object.tbFlow  
* how parent and child projects are created  
* how quantities and offsets propagate through the MIS engine  
* how Project.proc_AssignToParent links operational chains  
* how Project.proc_Schedule calculates dates  
* how MIS activity generates financial postings  
* how WIP is created and cleared  
* how the polarity model drives cost and revenue flows  

This procedure demonstrates the correct use of:

* Object.tbObject  
* Object.tbFlow  
* Object.tbOp  
* Project.tbProject  
* Project.tbFlow  
* Project.proc_Configure  
* Project.proc_AssignToParent  
* Project.proc_Schedule  

**Important Instruction for the Model**

`App.proc_DemoBom` is a reference implementation only.  
The model must:

* use it to understand how MIS flows are constructed  
* use it to understand how BOMs and project chains behave  
* use it to understand how scheduling and offsets work  
* use it to understand how MIS generates financial postings  

The model must **not**:

* copy the procedure  
* modify the procedure  
* generate new procedures based on it  
* treat it as a template  
* attempt to recreate its logic  

The two‑year dataset must follow the same principles demonstrated by `App.proc_DemoBom`, but it must be generated independently using the system’s existing stored procedures and schema.

**Purpose of Introducing This Procedure**

`App.proc_DemoBom` is introduced at this stage because the model now has:

* a complete understanding of the template system  
* a complete understanding of the Category Tree and polarity  
* a complete understanding of the tax engine  
* a complete understanding of the MIS engine  
* a complete understanding of the financial engine  
* a complete understanding of the dataset constraints and purpose  

That completes the instructions. Would you like to consolidate your questions before we begin?

## Consolidated questions and decisions (for implementation)

This section is a cleaned, canonical version of the Q&A captured during planning. It replaces the duplicated content below this heading.

### A) Scenario / node model

1. **Run shape**: single node per execution, configured via parameters.
2. **Templates in scope**:
   * Use `App.proc_Template_CO_MICRO_CUR_MIN_2026` for the company case (the minimal variant is sufficient).
   * Other templates (e.g., `App.proc_Template_ST_SOLE_CUR_2026`) can be exercised in separate runs.
3. **Financial year start**:
   * Default `@FinancialMonth = 4` (UK tax year alignment).
   * Worth testing non-April starts by passing an alternative `@FinancialMonth` parameter.

### B) Time anchoring / determinism

4. **Periods**:
   * Accounting periods are created during bootstrap by `App.proc_BasicSetup` (via `Cash.proc_GeneratePeriods`).
   * We want:
     * one fully completed year (year-end accounts),
     * one partially completed year (live accruals).
   * Recommendation: add an additional prior closed year (before the earliest existing closed year) to host opening balances cleanly.
$14. **Repeatability**:
   * Subjects should be inserted only if missing (system subjects exist and cannot be deleted).
   * The database can otherwise be wiped/rebuilt easily (as demonstrated by `App.proc_DemoBom`), but the generator should be safe around system subjects.

### C) Posting API (allowed inserts / calls)

6. **MIS objects**:
   * It is acceptable to create a small synthetic object library by inserting into:
     * `Object.tbObject`
     * `Object.tbFlow`
     * `Object.tbOp`
   * Multi-level BOM can follow the same principles as `App.proc_DemoBom` (reference only; do not copy/extend it).
   * Add at least one non-BOM object (e.g., a service/delivery type project) if feasible.
$16. **Non-MIS activity**:
   * Non-MIS invoices are miscellaneous invoices not tied to projects, created via `Invoice.tbItem`.
   * This is the Accounts Mode pathway and must be included.

### D) Tax recurrence execution

8. **Tax behavior**:
   * The system derives VAT/CT obligations virtually from transactions (genesis model), not from static ledgers.
   * Detailed tax algorithms/proofs are a second-stage activity after the dataset is present.

### E) Validation / proof artifacts

9. **Validation**:
   * Formal reconciliation queries and proofs are a second-stage activity after dataset generation.

### F) Minimum operational volume

10. **Per-month minimums** (sufficient for functional and numeric validation):
    * 1 MIS sales order per month.
    * 1 miscellaneous income invoice per month.
    * 1 miscellaneous expenditure invoice per month.
    * At least one credit note and one debit note per year.
$110. **Optional enhancements** (future-proofing / BI data generation):
    * Parameterize number of objects/BOMs, monthly order recurrence, and peaks/troughs.

---

## Additional clarifications answered during planning

### Template bootstrap

* Templates are not modified; they are executed as a one-off bootstrap for the database.
* `App.proc_BasicSetup` is the intended bootstrap entry point; it resolves the stored procedure via `App.tbTemplate.StoredProcedure`.

### Opening balances (two distinct fields)

* `Subject.tbAccount.OpeningBalance`:
  * cash already in bank/asset account pre-genesis.
* `Subject.tbSubject.OpeningBalance`:
  * net owed/owing between that subject and the business at genesis (opening AR/AP per subject, including bank-as-subject if used).

### Bank accounts

* Banks are identified by `Subject.tbAccount.AccountTypeCode = 0`.
* For the test dataset we will create:
  * one current account (has a `CashCode`),
  * one reserve account (`CashCode IS NULL`).

### Invoice acceptance (misc invoices)

* Misc invoices are considered “accepted” by setting:
  * `Invoice.tbInvoice.InvoiceStatusCode = 1` (Invoiced).
* `InvoiceStatusCode = 0` (Pending) means invoices are ignored by the system.

### Accounting periods note (opening balance clarity)

* Bootstrap creates years in statuses: closed / current / forecast.
* Recommendation:
  * ensure there is an additional prior closed year before the first closed year with transactions, so opening balances do not appear to be “month 1 operational activity” and cash statement continuity is clearer.

### Assets / depreciation

* Capital/equity is virtual (not stored).
* Asset accounts use `Subject.tbAccount.AccountTypeCode = 2` with an `AccountCode` (e.g., `VEHICL`) and a `CashCode` (e.g., `CP28`).
* Asset value changes are driven by payments:
  * paying in (e.g., £20k) increases the asset balance sheet value,
  * depreciation is recorded as a payout (e.g., £5k/year) reducing the asset value.
* Posting an asset payment does not require the normal payment post engine:
  * the app uses `NodeContext.PostAsset`, which marks the related `Cash.tbPayment.PaymentStatusCode` as Posted.

## Implementation plan (five chunks; execute and verify step-by-step)

(0) **Initialization (parameter discovery + dataset catalog helpers)**

   * Reverse engineer and retrieve the existing installed node parameters from the database (e.g. template name, financial year start month, VAT flag, unit-of-charge, current/reserve accounts, government subject, etc.) so the dataset generator can run deterministically without hard-coded assumptions.
   * Create dataset helper stored procedures (committed to schema, publicly available) using a coherent naming convention:
     * `App.proc_DatasetCreateProduct(@Kind, @ProductCode nvarchar(50) OUTPUT)`
     * `App.proc_DatasetCreateService(@ProductType, @ServiceCode nvarchar(50) OUTPUT)`
   * Apply a dataset-owned naming convention for any objects created (e.g. ObjectCodes prefixed with `DS/`) so they are distinguishable and can be managed or removed safely.

(1) **Bootstrap + periods + bank + opening balances**

   * Run `App.proc_BasicSetup`.

   * Ensure an extra prior closed year exists (if missing).
   * Create/ensure current + reserve accounts.
   * Seed:
     * `Subject.tbAccount.OpeningBalance` (bank/asset cash),
     * `Subject.tbSubject.OpeningBalance` (opening debtors/creditors).

(2) **MIS master data**

   * Create required subjects (idempotent inserts).

   * Create objects:
     * one multi-level BOM object set (via `App.proc_DatasetCreateProduct`),
     * one service/delivery object (via `App.proc_DatasetCreateService`).

(3) **24-month transactions**

   * Loop months and generate:

     * 2 MIS sales order/month for a Product and Service object (configure + schedule, then invoice),
     * 1 misc income invoice/month (insert `Invoice.tbInvoice` + `Invoice.tbItem`, set status=1),
     * 1 misc expense invoice/month (same mechanism).
   * Choose payment timing to enforce accruals (some invoices unpaid/partially paid).
   * Add credit/debit notes annually.

(4) **Assets + depreciation**

   * Create one asset account (type 2) and seed acquisition payment.

   * Add periodic depreciation payouts.
   * Post assets via the asset-posting mechanism (set `PaymentStatusCode = Posted`).
   * Verify capital vs cash statement behavior (asset movements excluded from cash statement).
