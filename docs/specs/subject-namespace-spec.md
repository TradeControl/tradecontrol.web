# Subject Namespace Specification

Copyright Trade Control Ltd. 27 April 2026.

> [Cannonical Source](https://tradecontrol.github.io/articles/tc_production/#organisations)

## Context and Purpose {#context}

This document defines the changes required to enhance the **Subject model** across both the data layer and the UI layer.  
The implementation work is divided into two phases:

- **Phase 1** — structural and behavioural changes within **tcNodeDb4**  
- **Phase 2** — UI, workflow, and interaction changes within **TCWeb**

Both phases must be completed for the enhanced Subject model to function correctly across the platform.

Additional behavioural rules governing the Subject Namespace are defined in the [Semantics](#semantics-overview) section later in this document.  
These rules must be understood when interpreting both Phase 1 and Phase 2.

# Index

- [Current Behaviour](#current-behaviour)
- [Goal](#goal)
- [Definition — Namespace](#definition-namespace)
- [Why This Change Is Required](#why-required)
- [Impact Summary](#impact-summary)
- [Deterministic Identity](#deterministic-identity)
- [Subject Classification](#subject-classification)
- [Inheritance](#inheritance)
- [Synthetic Dataset](#synthetic-dataset)
- [Composite Semantics](#composite-semantics)

## Phase 1 — Back-end (tcNodeDb4)

- [Phase 1 - Overview](#phase1-overview)
- [Phase 1 — Operating Principles](#phase1-operating-principles)
- [Phase 1 — Step 1: SubjectCode Rules](#phase1-step1)
- [Phase 1 — Step 2: Schema Design Changes](#phase1-step2)
- [Phase 1 — Step 3: Bootstrap Updates](#phase1-step3)
- [Phase 1 — Step 4: Synthetic Dataset Upgrade](#phase1-step4)
- [Phase 1 — Step 5: Compatibility View](#phase1-step5)
- [Phase 1 — Step 6: Clean‑Up](#phase1-step6)

## Phase 2 — Front-end (TCWeb)

- [Phase 2 - Overview](#phase2-overview)
- [Phase 2.1 — Model Integration](#phase21)
- [Phase 2.2 — Remove Legacy Code](#phase22)
- [Phase 2.3 — Substitute tbContact](#phase23)
- [Phase 2.4 — Subject Tree UI](#phase24)
    - [2.4.1 Conceptual Overview](#phase241)
    - [2.4.2 Purpose of the Subject Tree UI](#phase242)
    - [2.4.3 Functional Specification](#phase243)
    - [2.4.4 Component Interaction Flow](#phase244)
    - [2.4.5 Data Loading Contracts](#phase245)
- [Phase 2.5 — Namespace Selector](#phase25)
- [Phase 2.6 — Subject Detail Panel](#phase26)
- [Phase 2.7 — Namespace Semantics](#phase27)
- [Phase 2.8 — Subject Maintenance](#phase28)
    - [2.8.1 - Maintenance Workflow](#phase281)
    - [2.8.2 - Address Management Workflow](#phase282)
- [Phase 2.9 — Validation and Guardrails](#phase29)
- [Phase 2.10 — Component Responsibilities and Boundaries](#phase210)

## Semantics

- [Semantics - Overview](#semantics-overview)
- [Semantics 3.1 — Namespace Identity](#semantics31)
- [Semantics 3.2 — Parent/Child Relationship Rules](#semantics32)
- [Semantics 3.3 — Namespace Modification Operations](#semantics33)
- [Semantics 3.4 — DAG Validation Rules](#semantics34)
- [Semantics 3.5 — Namespace Path Resolution](#semantics35)
- [Semantics 3.6 — Filtering and Search Semantics](#semantics36)
- [Semantics 3.7 — UI Workflows for Namespace Editing](#semantics37)
- [Semantics 3.8 — Performance and Consistency Guarantees](#semantics38)
- [Semantics 3.9 — Error Handling for Namespace Operations](#semantics39)
- [Semantics 10 — Summary](#semantics310)

## Current Behaviour {#current-behaviour}

Subjects are currently modelled using a **two-tier hierarchy** in a `Subject` schema:

- Organisations are stored in `Subject.tbSubject`
- Contacts belonging to an organisation are stored in `Subject.tbContact`

This structure enforces a rigid **Organisation → Contact** relationship.  
It cannot represent deeper organisational structures, internal departments, roles, teams, or conceptual groupings.

## Goal {#goal}

Replace the existing two-tier model with a flexible, recursive structure called a **Namespace**.

## Definition — Namespace {#definition-namespace}

A **Namespace** is a hierarchical network of Subjects.  
It behaves similarly to a .NET namespace:

- Each Subject has a name  
- Subjects may have **zero or more parents**  
- Subjects may have **zero or more children**  
- The hierarchy is **not limited to two levels**  
- The full path of a Subject expresses **structure and meaning**

Examples:

```csharp
    BellMaker.Foundry.Production
    BellMaker.Foundry.Production.Maintenance
    BellMaker.Foundry.Production.Stores.PrimaryMaterials
    BellMaker.Foundry.Production.Stores.Components
    BellMaker.Foundry.Production.ToolRoom
    BellMaker.Foundry.Production.ShopFloor
    BellMaker.Foundry.Production.Warehouse.GoodsInwards
    BellMaker.Foundry.Production.Warehouse.Despatch
```

A Namespace allows Subjects to represent:

- Organisations  
- People  
- Departments  
- Teams  
- Roles  
- Conceptual units  
- Any structure required by the business  

## Why This Change Is Required {#why-required}

- Real businesses do not fit into a two-level Organisation → Contact model  
- Subjects must be able to participate in **arbitrary structures**  
- The system must support **recursive navigation**  
- The system must support **inheritance of defaults**  
- The system must support **multiple Subject classes** (Real, Virtual, Structural)  
- The system must support **contextual behaviour** based on Namespace position  

## Impact Summary {#impact-summary}

- `tbContact` will be removed  
- People will become **Subjects**  
- A **Namespace table** will be introduced  
- New **Subject extension tables** will be introduced  
- A **compatibility view** will replace `tbContact` during transition  
- The **synthetic dataset** will be updated to use the new model  
- The UI will gain:
      - a **Subject Tree**
      - a **Namespace Selector**

## Deterministic Identity {#deterministic-identity}

All Subjects must continue to use deterministic codes generated via  
`App.proc_DefaultCodeGenerator`.

Identity seeds must not be used.

## Subject Classification {#subject-classification}

Subject behaviour is determined by:

- **SubjectTypeCode** (user-configurable)
- **CashPolarityCode** (financial behaviour)
- **SubjectClass** (Real, Virtual, Structural)

A **polarity/class matrix** will be provided to determine which Subjects should appear in the compatibility view that replaces `tbContact`.

## Inheritance (Phase 1 Scope) {#inheritance}

During Subject creation:

- Default common fields from the parent Subject  
- If the SubjectClass is **Real**, default `CashPolarityCode = Neutral`  
- Full inheritance logic will be implemented later in the financial and workflow schemas  

## Synthetic Dataset {#synthetic-dataset}

The synthetic dataset is an **artificial test environment** used for:

- verification  
- regression testing  
- tutorials  
- validating schema changes  

If the synthetic dataset works, real datasets will work because they use the same algorithms.

The synthetic dataset must be updated to use the new Namespace model before `tbContact` is removed.

# Composite Semantics {#composite-semantics}

## Purpose

This section explains the **composite semantics** that govern Subject behaviour in the enhanced tcNodeDb4 schema. These semantics combine three independent dimensions:

- **SubjectTypeCode** — the user-defined type of the Subject  
- **SubjectClassCode** — the structural class of the Subject (Real, Virtual, Structural)  
- **CashPolarityCode** — the financial polarity associated with the Subject  

Together, these three values determine:

- how a Subject behaves in financial transactions  
- how a Subject participates in workflows  
- how a Subject appears in the compatibility view that replaces `tbContact`  
- how Subjects are interpreted within the Namespace  
- how defaulting rules apply during Subject creation  

## Conceptual Overview

### 1. SubjectTypeCode  

A SubjectType defines the *role* or *category* of a Subject.  
Examples include:

- Customer  
- Supplier  
- Employee  
- Department  
- Wallet  
- Asset Ledger  
- Structural node  

SubjectType is **user-configurable** and determines the high-level behaviour of the Subject.

### 2. SubjectClassCode

A SubjectClass defines the *structural nature* of the Subject.  
There are three classes:

- **Real** — represents a real-world entity (person, organisation, asset)  
- **Virtual** — represents a conceptual or logical entity (ledger, wallet, cost centre)  
- **Structural** — represents a Namespace node used for grouping and inheritance  

SubjectClass determines:

- whether the Subject participates in financial polarity  
- whether the Subject appears in the compatibility view  
- how defaulting rules apply  
- how the Namespace interprets the node  

### 3. CashPolarityCode

CashPolarity defines the financial direction associated with the Subject.  
Cash polarity is hard coded as:

- Expense
- Income
- Neutral  

Polarity is **orthogonal** to SubjectClass.  
For example:

- A Real Subject (e.g., a person) should default to Neutral, but could be Expense (e.g. an employee)
- A Virtual Subject may be Income (e.g., a customer) or Expense (e.g. a supplier)  
- A Structural Subject has no polarity and should be Neutral  

## Composite Semantics Matrix

The combination of:

- **SubjectClassCode**  
- **CashPolarityCode**  

determines whether a Subject should appear in the compatibility view that replaces `tbContact`.

The Model will be provided with a matrix mapping each `(SubjectClassCode, CashPolarityCode)` pair to a boolean indicating whether the Subject is considered a “contact-equivalent” entity.

This allows The Model to infer:

- which Subjects represent people  
- which Subjects represent organisations  
- which Subjects represent conceptual nodes  
- which Subjects should appear in the compatibility view  
- which Subjects should not

The demonstration script below shows a minimal example of how these semantics are represented in a blank database.  

**The Model must interpret these semantics and apply them to the real Business Node schema.**

## Demonstration Script — Composite Semantics Prototype  

### Purpose

This standalone T‑SQL script demonstrates, in a blank database, the **composite semantics** required for the enhanced Subject model in tcNodeDb4.

It introduces:

- `Cash.tbPolarity` — defining financial polarity  
- `Subject.tbClass` — defining Subject classes (Real, Virtual, Structural)  
- `Subject.tbType` — defining Subject types, each carrying:  
-
  - a `CashPolarityCode`  
  - a `SubjectClassCode`  

This script is **not** the Business Node schema.  
It is a **minimal conceptual demonstration** that The Model must interpret and map onto the real tcNodeDb4 database during refactoring.

## SQL Demonstration Script

```sql
CREATE SCHEMA [Subject];
go
CREATE SCHEMA [Cash];
go
CREATE TABLE [Cash].[tbPolarity] (
    [CashPolarityCode] SMALLINT      NOT NULL,
    [CashPolarity]     NVARCHAR (10) NULL,
    CONSTRAINT [PK_Cash_tbPolarity] PRIMARY KEY CLUSTERED ([CashPolarityCode] ASC) WITH (FILLFACTOR = 90)
);
go
CREATE TABLE [Subject].[tbType] (
    [SubjectTypeCode] SMALLINT      CONSTRAINT [DF_Subject_tbType_SubjectTypeCode] DEFAULT ((1)) NOT NULL,
    [CashPolarityCode]         SMALLINT      NOT NULL,
    [SubjectType]     NVARCHAR (50) NOT NULL,
    [RowVer]               ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Subject_tbType] PRIMARY KEY NONCLUSTERED ([SubjectTypeCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Subject_tbType_Cash_tbPolarity] FOREIGN KEY ([CashPolarityCode]) REFERENCES [Cash].[tbPolarity] ([CashPolarityCode])
);
go
CREATE TABLE Subject.tbClass
(
    SubjectClassCode smallint NOT NULL,
    SubjectClass nvarchar(50) NOT NULL,
    CONSTRAINT PK_Cash_tbSubjectClass PRIMARY KEY CLUSTERED (SubjectClassCode ASC)
) 
go
INSERT INTO Subject.tbClass (SubjectClassCode, SubjectClass)
VALUES (0, 'Virtual'), (1, 'Real'), (2, 'Structural');

INSERT INTO Cash.tbPolarity (CashPolarityCode, CashPolarity)
VALUES (0, 'Expense')
, (1, 'Income')
, (2, 'Neutral');

go
ALTER TABLE Subject.tbType WItH NOCHECK ADD
    SubjectClassCode smallint NOT NULL CONSTRAINT DF_Subject_tbType_SubjectClassCode DEFAULT (0)
go
ALTER TABLE Subject.tbType  WITH CHECK ADD 
    CONSTRAINT FK_Subject_tbType_tbClass FOREIGN KEY(SubjectClassCode)
    REFERENCES Subject.tbClass (SubjectClassCode)
go

INSERT INTO Subject.tbType (SubjectTypeCode, CashPolarityCode, SubjectClassCode, SubjectType)
VALUES (0, 0, 0, 'Supplier')
, (1, 1, 0, 'Customer')
, (2, 1, 0, 'Prospect')
, (4, 1, 0, 'Company')
, (5, 0, 0, 'Bank')
, (6, 0, 1, 'Employee')
, (7, 2, 2, 'Department');
go

```

# Phase 1 — Detailed Specification {#phase1-overview}

This section defines the **structural**, **behavioural**, and **migration** steps required to introduce the enhanced Subject model into tcNodeDb4.

All instructions in this section must be followed **exactly** and in the order presented.

> **IMPORTANT** This phase must be interpreted in conjunction with the [Semantics](#semantics-overview) section, which defines the behavioural rules governing the Subject Namespace.

# Phase 1 — Operating Principles {#phase1-operating-principles}

These rules apply to **all** steps in Phase 1.

## Deterministic Identity

- All Subjects must use `App.proc_DefaultCodeGenerator`.
- No identity seeds, GUIDs, or non‑deterministic keys.
- All extension tables must use `SubjectCode` as PK/FK.

## Composite Semantics

- Composite semantics live **only** on `Subject.tbType`.
- `Subject.tbSubject` must not contain:
  - `SubjectClassCode`
  - `CashPolarityCode`
- These values are derived via the join:

tbSubject → tbType → (SubjectClassCode, CashPolarityCode)

## Subject Schema Structure

The Subject schema consists of:

- `Subject.tbSubject` — identity + common fields  
- `Subject.tbReal` — refactored from `tbContact`  
- `Subject.tbVirtual` — organisation‑specific fields refactored out of `tbSubject`  
- `Subject.tbStructural` — stub table (SubjectCode + Notes)

All three extension tables are **one‑to‑one** with `tbSubject`.

## Namespace

- The Namespace table defines recursive parent/child relationships.
- No data migration occurs in Phase 1.
- Namespace roots must be created during bootstrap.

## Defaulting Rules

- On Subject creation:
- Inherit common fields from parent.
- If SubjectClass = Real → default CashPolarity = Neutral.
- Full inheritance logic is deferred to later phases.

## Demonstration Script

- The demonstration SQL is **conceptual**, not literal.
- The Model must interpret the semantics and apply them to the real schema.

## No Data Migration in Phase 1

- Only structural changes + compatibility view.
- Real migration occurs after UI changes in Phase 2.

# Phase 1 INSTRUCTIONS (High-Level Overview)

1. Widen `SubjectCode` to `nvarchar(50)` and update the default code generator to produce whole-word SubjectCodes for all new Subjects.
2. Introduce the new Subject model (Namespace + extension tables).
3. Amend the bootstrap the reflect the schema changes.
4. Update the synthetic dataset to use the new model and whole-word SubjectCodes.
5. Create a compatibility view that exposes the same columns as `tbContact` and redirect all dependencies.
6. Remove the physical `tbContact` table once the system compiles and runs.

# Phase 1 - Step 1 {#phase1-step1}

## SubjectCode Length, Whole‑Word Generation, and Identity Rules

The SubjectCode is the stable, immutable identifier for every Subject.  
It is not the namespace path; it is the name of the Subject.  
Namespace paths are constructed by concatenating SubjectCodes using dot‑notation.

To support human‑readable namespaces and future UX improvements, the SubjectCode length is increased from `nvarchar(10)` to `nvarchar(50)`.

### SubjectCode column definition

All Subject tables and extension tables must use:

`SubjectCode nvarchar(50) NOT NULL`

All foreign keys referencing SubjectCode must be updated accordingly.

### Whole‑word SubjectCodes

All new Subjects created by the platform must use whole‑word SubjectCodes generated from the Subject’s description or identity fields.

Examples:

``` text
WinServices
HumanResources
PersonnelDept
ChristopherRobin
```

Legacy 10‑character codes remain valid and supported, but are not used for new Subjects.

### Updated code generator contract

The default code generator is updated to support whole‑word generation:

``` sql
ALTER PROCEDURE App.proc_DefaultCodeGenerator
(
    @Description nvarchar(100),
    @CheckSql nvarchar(max),          -- SQL must accept parameter @Code and return COUNT(*) into @cnt OUTPUT
    @UseWholeWords bit = 0,
    @Code nvarchar(50) OUTPUT
)
```

### Generator behaviour

When `@UseWholeWords = 1`:

- The generator derives a whole‑word token from `@Description`
- Illegal characters are removed or normalised
- Uniqueness is enforced using `@CheckSql`
- Suffixes may be added if required
- The generator remains deterministic

When `@UseWholeWords = 0`:

- The legacy 10‑character deterministic format is used
- This mode is retained only for backwards compatibility

### Namespace construction

Namespace paths are constructed by concatenating SubjectCodes with dots:

`ParentCode.ChildCode.GrandchildCode`

The parser operates on dot‑separated segments and is unaffected by the increased SubjectCode length.

### Migration note

Because SubjectCode is a primary key and appears in multiple foreign keys, the schema change must be applied consistently across related tables.

The Model may request human assistance to perform a global search‑and‑replace of `SubjectCode nvarchar(10)` to `SubjectCode nvarchar(50)` across the schema, as this is a structural change affecting primary keys and foreign keys.

# Phase 1 — Step 2 {#phase1-step2}

## Schema Design Changes

### 2.1 Introduce Composite Semantics

Modify `Subject.tbType` to include:

- `SubjectClassCode` (FK → `Subject.tbClass`)
- `CashPolarityCode` (FK → `Cash.tbPolarity`)

Ensure:

- All existing SubjectTypes are mapped to class/polarity pairs.
- The polarity/class matrix is available for compatibility view logic.

### 2.2 Introduce Namespace Table

Create:

``` text
Subject.tbNamespace
(
    ParentSubjectCode  FK → tbSubject,
    ChildSubjectCode   FK → tbSubject,
    Ordinal            int NOT NULL,
    PK (ParentSubjectCode, ChildSubjectCode)
)
```

No data is inserted in Phase 1 except bootstrap roots.

### 2.3 Refactor `Subject.tbSubject`

`tbSubject` becomes the **identity + common fields** table.

Remove fields that belong specifically to virtual entities and move them into `Subject.tbVirtual`.

Retain:

- `SubjectCode`
- `SubjectTypeCode`
- common fields (email, phone, status, audit, etc.)

### 2.4 Refactor `Subject.tbContact → Subject.tbReal`

Create:

``` text
Subject.tbReal
(
    SubjectCode PK/FK → tbSubject,
    <fields refactored from tbContact>
)
```

Move all person‑specific fields from `tbContact` into `tbReal`.

### 2.5 Create `Subject.tbVirtual`

Create:

``` text
Subject.tbVirtual
(
    SubjectCode PK/FK → tbSubject,
    <fields refactored from tbSubject that apply only to virtual entities>
)
```

This table receives the organisation‑specific fields previously stored in `tbSubject`.

### 2.6 Create `Subject.tbStructural` (Stub)

Create:

``` text
Subject.tbStructural
(
    SubjectCode PK/FK → tbSubject,
    Notes nvarchar(max) NULL
)
```

No additional fields in Phase 1.

# Phase 1 — Step 3 {#phase1-step3}

## Node Bootstrap Updates

Update `App.proc_NodeDataInit`, `App.proc_NodeBusinessInit` to:

- Insert Subject.tbClass
- Create default SubjectTypes with class/polarity assignments.
- Create default Subjects using the new schema.
- Insert Namespace root nodes.
- Ensure all Subjects created during bootstrap populate:
    - `tbSubject`
    - the appropriate extension table (`tbReal`, `tbVirtual`, or `tbStructural`)

Bootstrap must succeed in a blank Business Node.

# Phase 1 — Step 4 {#phase1-step4}

## Synthetic Dataset Upgrade

Update the synthetic dataset generator to:

- Create Subjects using the new schema.
- Populate:
    - `tbSubject`
    - `tbReal`
    - `tbVirtual`
    - `tbStructural`
    - `tbNamespace`
- Use deterministic identity generation.
- Ensure the dataset compiles and produces a coherent world.

The synthetic dataset is the verification harness for Phase 1.

# Phase 1 — Step 5 {#phase1-step5}

## Compatibility View for `tbContact`

### 5.1 Identify Dependencies

Locate all references to:

- `Subject.tbContact`
- any stored procedures, functions, or views that depend on it
- UI components that expect contact‑like entities

### 5.2 Create Compatibility View

Create a view named:

`Subject.vwContactBase`

This view must:

- Expose the **same column names** as `tbContact`.
- Populate from:
      - `tbSubject`
      - `tbReal`
      - `tbType`
- Use the composite semantics matrix to determine which Subjects appear.

### 5.3 Redirect Dependencies

Replace all references to `tbContact` with `vwContact`.

Before replacing `Subject.tbContact` with the compatibility view (`Subject.vwContact`), The Model must update all database objects that reference the physical table. The following list represents the **complete set of dependencies** identified in the Business Node schema.

This list is authoritative.  

The Model must use it to:

- redirect each dependency to `Subject.vwContact`
- ensure all objects compile successfully after substitution
- confirm no remaining references to the physical table exist
- only then proceed to drop `Subject.tbContact`

#### Dependency List

| ObjectType             | SchemaName | ObjectName                          |
|------------------------|------------|--------------------------------------|
| SQL_SCALAR_FUNCTION    | Project    | fnEmailAddress                       |
| SQL_STORED_PROCEDURE   | App        | proc_DemoServices                    |
| SQL_STORED_PROCEDURE   | Project    | proc_Configure                       |
| SQL_STORED_PROCEDURE   | Project    | proc_EmailAddress                    |
| SQL_STORED_PROCEDURE   | Project    | proc_EmailDetail                     |
| SQL_STORED_PROCEDURE   | Subject    | proc_AddContact                      |
| SQL_STORED_PROCEDURE   | Subject    | proc_DefaultEmailAddress             |
| SQL_TRIGGER            | Project    | Project_tbProject_TriggerInsert      |
| SQL_TRIGGER            | Project    | Project_tbProject_TriggerUpdate      |
| SQL_TRIGGER            | Subject    | Subject_tbContact_TriggerInsert      |
| SQL_TRIGGER            | Subject    | Subject_tbContact_TriggerUpdate      |
| TRIGGER                | Project    | Project_tbProject_TriggerInsert      |
| TRIGGER                | Project    | Project_tbProject_TriggerUpdate      |
| TRIGGER                | Subject    | Subject_tbContact_TriggerInsert      |
| TRIGGER                | Subject    | Subject_tbContact_TriggerUpdate      |
| VIEW                   | Project    | vwDoc                                |
| VIEW                   | Project    | vwPurchaseEnquiryDeliverySpool       |
| VIEW                   | Project    | vwPurchaseEnquirySpool               |
| VIEW                   | Project    | vwPurchaseOrderDeliverySpool         |
| VIEW                   | Project    | vwPurchaseOrderSpool                 |
| VIEW                   | Project    | vwQuotationSpool                     |
| VIEW                   | Project    | vwSalesOrderSpool                    |
| VIEW                   | Subject    | vwContacts                           |
| VIEW                   | Subject    | vwDepartments                        |
| VIEW                   | Subject    | vwEmailAddresses                     |
| VIEW                   | Subject    | vwJobTitles                          |
| VIEW                   | Subject    | vwMailContacts                       |
| VIEW                   | Subject    | vwNameTitles                         |

1. For each object listed above, replace references to `Subject.tbContact` with `Subject.vwContact`.
2. Recompile each object after substitution.
3. Confirm that no object in the dependency list references the physical table.
4. Confirm that no *other* object in the database references the physical table.
5. Only when all dependencies are resolved and the system compiles cleanly may the physical table be dropped.

This list is a mandatory prerequisite for the safe removal of `Subject.tbContact`.

### 5.4 Remove Physical Table

Once all dependencies compile and run:

- Drop `Subject.tbContact`.
- Retain `vwContact` as the compatibility layer.

---

# Phase 1 — Step 6 {#phase1-step6}

## Clean‑Up

Remove:

- obsolete columns from `tbSubject`
- obsolete constraints
- unused indexes
- any logic tied to the old Organisation → Contact model

Ensure:

- all Subject creation logic uses the new schema
- the Namespace is the authoritative structure
- the synthetic dataset and bootstrap both run cleanly

# Phase 2 — TCWeb Integration Outline {#phase2-overview}

Phase 2 applies the enhanced Subject schema to the **TCWeb** application.  
The work proceeds in the following order:

1. **Model Integration**
   - Add the new Subject models (`tbSubject`, `tbReal`, `tbVirtual`, `tbStructural`, `tbType`, `tbNamespace`, `vwContactBase`).
   - Instantiate all models in `NodeContext`.
   - Ensure correct bindings, navigation properties, and one‑to‑one relationships.

2. **Remove Legacy Code**
   - Delete obsolete Subject UI folders.
   - Retain only `Invoice`, `Payments`, and `Statement` under Enquiry.
   - Leave `System.Reports` untouched.
   - Replace the root Subject Index with a search redirect into `SubjectTree/Index`.

3. **Substitute `tbContact` with `vwContactBase`**
   - Replace all remaining references to `tbContact`.
   - Update any queries, models, or UI components that depend on contact data.
   - Ensure compatibility with the new Subject identity model.

4. **Subject Tree UI**
   - Implement the new hierarchical Subject browser.
   - Support structural, real, and virtual Subjects.
   - Provide filtering, selection, expansion, and navigation.
   - Integrate with the new models and `vwContactBase`.

5. **Namespace Selector Control**
   - Implement a reusable UI component for selecting a Subject Namespace.
   - Support single‑select and multi‑select modes.
   - Integrate with the Subject Tree and all relevant workflows.

6. **Workflow Updates**
   - Update any remaining pages that interact with Subjects.
   - Ensure all workflows use the new Subject identity model.

7. **Clean‑Up**
   - Remove any unused helpers, partials, or legacy bindings.
   - Ensure consistency across the TCWeb UI.

> **IMPORTANT** This phase must be interpreted in conjunction with the [Semantics](#semantics-overview) section, which defines the behavioural rules governing the Subject Namespace.

## Phase 2.1 — Model Integration (TCWeb) {#phase21}

Phase 2 begins with the integration of the enhanced Subject schema into the **TCWeb** application layer.  
Before any UI or workflow changes can be implemented, The Model must introduce the updated data models and ensure they are correctly instantiated within the `NodeContext`.

### Required Models

The following models must be created or updated to reflect the Phase 1 schema:

- `Subject.tbSubject`  
- `Subject.tbReal`  
- `Subject.tbVirtual`  
- `Subject.tbStructural`  
- `Subject.tbType` (including composite semantics)  
- `Subject.tbNamespace`  
- `Subject.vwContactBase` (new compatibility view model)

Each model must map **all fields** defined in the Phase 1 schema, including:

- identity fields  
- common fields  
- one‑to‑one extension fields  
- composite semantics (via `SubjectTypeCode`)  
- namespace relationships  
- compatibility view projections  

### NodeContext Integration

The Model must instantiate all Subject‑related models within the `NodeContext`, ensuring:

- correct table bindings  
- correct view bindings  
- correct navigation properties  
- correct one‑to‑one relationships  
- correct foreign key relationships  
- correct namespace parent/child collections  
- correct exposure of `vwContactBase` as the replacement for legacy contact queries  

This integration step is mandatory before any UI components, selectors, or workflows can be updated to use the new Subject model.

Phase 2 UI development depends on the successful completion of this model integration.

## Phase 2.2 — Remove Legacy Code {#phase22}

Before integrating the new Subject models or substituting `tbContact` with `vwContactBase`, all obsolete UI components in **TCWeb** must be removed. This reduces the number of references to update and ensures that subsequent changes are applied cleanly and deterministically.

### 1. Delete the Following Folders Entirely (TCWeb / Subject)

These folders are based on the legacy Subject schema and must be removed:

- `Subject/Address`
- `Subject/Contact`
- `Subject/Update`

### 2. Enquiry Folder — Retain Only Three Pages

Keep:

- `Invoice.cshtml`
- `Payments.cshtml`
- `Statement.cshtml`

Delete all other pages in `Subject/Enquiry`.

### 3. Reports

- Leave the **`System.Reports`** folder untouched.  
  It contains the **Debtors and Creditors** report, which must be retained.

- Delete any **Subject‑level** reports that are not explicitly retained above.

### 4. Subject.Type

Do **not** delete or relocate `Subject.Type`.

- It remains surfaced via the **Admin Manager**.
- The model will be modified (e.g., class, semantics).
- The Admin Manager UI will update automatically because it binds directly to the model.

### 5. Root Subject Index

Replace:

`Subject/Index.cshtml`

with a search page that redirects into:

`SubjectTree/Index`

with filtered results.

This becomes the new entry point for Subject navigation.

### 6. Remaining Legacy UI

Any remaining Subject UI that:

- lists  
- edits  
- filters  
- navigates  
- displays  

Subjects is removed and replaced by the new **Subject Tree UI** and **Namespace Selector Control**.

## Phase 2.3 - Substitute `tbContact` with `vwContactBase` {#phase23}

With the legacy Subject UI removed, the remaining references to `tbContact` are limited and can now be replaced cleanly.

### Required Steps

1. **Request human to perform a global search for:**

`Subject_tbContact`

and replace all occurrences with:

`Subject_vwContactBase`

2. **Record all affected areas.**  

The human must note each file and location where the substitution occurs.  
These areas will require verification after compilation.

3. **Resolve all compilation errors.**  

The human must work through each error in turn, updating:

- model bindings  
- view models  
- LINQ queries  
- navigation properties  
- controller actions  
- any remaining references to `tbContact` fields  

4. **Verify functional behaviour.**  

After each fix, the human must test that:

- the page loads  
- the data binds correctly  
- the expected Subject information is displayed  
- no legacy `tbContact` assumptions remain  

5. **Confirm compatibility with the new Subject identity model.**  

Ensure that all updated components now operate using:

- `SubjectCode`  
- `SubjectClassCode`  
- namespace relationships  
- the new `vwContactBase` projection  

This completes the replacement of the legacy `tbContact` table with the compatibility view introduced in Phase 1.

## Phase 2.4 - Subject Tree UI {#phase24}

## Phase 2.4.1 — Subject Tree UI (Conceptual Overview) {#phase241}

Before defining the UI, it is essential to understand the flexibility of the underlying Subject Namespace model.

### 1. The Namespace Model Is Fully User‑Defined

The system does not impose any organisational structure.  
Users may create:

- a single structural root (e.g. “Root” or their company name)
- multiple structural roots (one per organisation, region, division, etc.)
- a hybrid structure with several top‑level nodes
- no explicit root at all, attaching Subjects directly to virtual or structural nodes

All of these patterns are valid.  
The model supports a **forest**, not a single tree.

### 2. The UI Must Not Assume a Fixed Structure

Because the Namespace model is a DAG:

- there may be one root  
- there may be twenty‑five roots  
- there may be thousands of children under a single node  
- a Subject may appear in multiple places  
- the structure may be deep, shallow, wide, narrow, or inconsistent

The UI must therefore be:

- agnostic to the number of roots  
- capable of rendering a forest  
- capable of handling very large child sets  
- capable of lazy‑loading nodes on demand  
- capable of filtering before expansion  
- capable of searching across the entire namespace  
- capable of selecting a Subject without expanding the tree

### 3. Data Volume Drives UI Behaviour

The model allows extremely large structures.  
For example, a business may have:

- thousands of customers  
- hundreds of suppliers  
- thousands of outlets or locations  
- multiple organisations under one deployment

Expanding a node with thousands of children must **not** trigger a full render.  
The UI must:

- load children only when expanded  
- load children in pages if necessary  
- provide filtering before expansion  
- avoid blocking the UI thread  
- avoid rendering thousands of DOM nodes at once

### 4. One UI Must Support All Scenarios

The system must not enforce:

- “one root per organisation”
- “one global root”
- “one structure per tenant”

Users decide how to organise their Subjects.  
The UI must adapt to whatever structure they create.

This flexibility is a core advantage of the platform.

### 5. Summary

The Subject Tree UI must:

- render a forest, not a single tree  
- support arbitrary user‑defined structure  
- handle large datasets efficiently  
- load nodes lazily  
- support filtering and search  
- integrate with the new Subject identity model  
- remain responsive regardless of data volume

This conceptual foundation informs the detailed UI specification that follows.

### General Instructions (Critical)

Before implementing the Subject Tree UI, the following constraints apply.  
These instructions override any default behaviour of the model.

1. **Do not generate large JavaScript files.**  
   The existing Razor components (`TreeShell.razor`, `TreeBranch.razor`, `TreeNode.cs`) must be used.  
   No standalone JavaScript tree engines, no client‑side frameworks, and no auto‑generated scripts.

2. **Do not replace the existing Tree components.**  
   The Tree UI must extend and adapt the existing components, not recreate them.

3. **Do not attempt to render the entire tree at once.**  
   All nodes must be loaded lazily.  
   The UI must never attempt to expand thousands of children in one operation.

4. **Do not assume a fixed structure.**  
   The Namespace model supports:
   - one root  
   - many roots  
   - hybrid structures  
   - deep or shallow structures  
   - large or small datasets  
   The UI must adapt to whatever structure the user creates.

5. **The Namespace Selector must drive the Tree.**  
   Filtering the namespace must update the visible portion of the tree.  
   Typing a company name, region, or structural label must reduce the forest to relevant branches.

6. **The UI must remain responsive regardless of data volume.**  
   This requires:
   - lazy loading  
   - paging of large child sets  
   - filtering before expansion  
   - no blocking operations  
   - no full‑tree traversal on the client

7. **The Tree is a semantic browser, not a file explorer.**  
   It reflects the Subject Namespace model, not a fixed organisational hierarchy.

These constraints must be applied before any functional or structural work begins.

## Phase 4.2 - Purpose of the Subject Tree UI {#phase242}

The Subject Tree UI serves three distinct purposes.  
All three must be supported by the same component set (`TreeShell`, `TreeBranch`, `TreeNode`), without creating separate UIs or divergent code paths.

### 1. Read‑Only Enquiries and Reporting

The Tree must allow users to:

- browse the Subject hierarchy
- inspect Subjects and their details
- navigate to related enquiries (Invoices, Payments, Statement)
- support reporting workflows (e.g., Debtors & Creditors)

This mode is non‑destructive.  
It must be fast, responsive, and safe for high‑volume datasets.

### 2. Namespace Construction and Maintenance

The Tree must allow users to construct and modify the Subject Namespace:

- add new structural nodes
- reorganise branches
- attach Subjects to new parents
- create or remove namespace relationships

This is the “structural editing” mode.  
It must respect the DAG model and support multiple roots, hybrid structures, and arbitrary user‑defined organisation.

### 3. Subject Maintenance

The Tree must support Subject‑level operations:

- - create new Subjects (real, virtual, structural)
- edit Subject details
- change Subject type
- manage relationships and attributes
- integrate with `vwContactBase` for identity and contact data

This is the “entity editing” mode.  
It must be consistent with the new Subject identity model introduced in Phase 1.

### Unified UI Philosophy

All three purposes must be supported by the same UI framework.  
The Tree is not a single‑purpose component — it is the central navigation and maintenance surface for the entire Subject model.

The UI must therefore:

- remain responsive regardless of data volume  
- support lazy loading and filtering  
- integrate with the Namespace Selector  
- adapt to arbitrary user‑defined structures  
- avoid assumptions about organisational shape or number of roots  
- avoid generating large JavaScript files or client‑side engines  

These purposes inform the functional specification that follows.

## Phase 2.4.3 — Subject Tree UI (Functional Specification) {#phase243}

The Subject Tree UI must be implemented using a **Subject‑specific wrapper layer** that sits on top of the existing generic tree components located in:

``` text
Shared/Tree/
TreeShell.razor
TreeBranch.razor
TreeNode.cs
```

These Shared components **must not be modified**.  
All Subject‑specific behaviour must be implemented in a new namespace:

``` text
Subject/Tree/
SubjectTreeShell.razor
SubjectTreeBranch.razor
SubjectTreeNode.cs
```

This mirrors the architecture used by `Admin.Manager` and ensures the Shared.Tree components remain generic and reusable.

## 1. Component Architecture

### 1.1 SubjectTreeShell.razor

Acts as the **host** for the Subject Tree.  
Responsibilities:

- initialise the tree
- apply Namespace Selector filters
- load root nodes lazily
- manage UI mode (Enquiry / Namespace / Subject)
- coordinate loading of children via SubjectTreeBranch
- host the Subject detail panel
- pass data to Shared.TreeShell without modifying Shared.Tree

### 1.2 SubjectTreeBranch.razor

Represents a branch in the Subject tree.  
Responsibilities:

- lazy loading of children
- paging of large child sets
- applying namespace filters before loading children
- exposing Subject‑specific context menus based on mode
- passing generic node data to Shared.TreeBranch

### 1.3 SubjectTreeNode.cs

The Subject‑specific node model.  
Responsibilities:

- hold Subject identity data
- hold namespace path for filtering
- expose flags for structural / real / virtual
- expose child count for lazy loading
- map to the generic `TreeNode` used by Shared.Tree

## 2. Lazy Loading (Mandatory)

Lazy loading must be implemented **only** in the Subject‑specific layer.

### Required behaviour

- Root nodes load only when SubjectTreeShell initialises.
- Children load only when SubjectTreeBranch is expanded.
- If a node has many children:
      - load in pages (e.g., 50 at a time)
      - show a “Load more…” indicator
- Shared.Tree must never attempt to load or render children itself.

## 3. Namespace Selector Integration

The Namespace Selector is a **filtering lens** over the tree.

### Required behaviour

- As the user types, SubjectTreeShell recalculates the visible root set.
- All branches collapse when the filter changes.
- Only nodes whose namespace path matches the filter remain visible.
- Filtering applies before any lazy loading.
- Filtering must not modify data — only visibility.

Shared.Tree must not implement filtering.  
Filtering is entirely the responsibility of SubjectTreeShell and SubjectTreeBranch.

## 4. UI Modes

The Subject Tree supports three modes:

- **EnquiryMode** — read‑only browsing and reporting
- **NamespaceMode** — structural editing (add/remove/reparent)
- **SubjectMode** — Subject maintenance (edit, create, delete)

### Required behaviour

- SubjectTreeShell exposes the current mode.
- SubjectTreeBranch shows context menus appropriate to the mode.
- Shared.Tree remains unaware of modes.

## 5. Detail Panel

Selecting a node must load a detail panel hosted by SubjectTreeShell.

### Required behaviour

- Show Subject identity (via vwContactBase)
- Show namespace relationships
- Show available actions based on mode
- Load independently of tree expansion
- Must not require Shared.Tree modifications

## 6. Multi‑Parent Awareness

The Namespace model is a DAG.  
A Subject may appear under multiple branches.

### Required behaviour

- Each appearance is treated as a separate branch instance.
- Expanding one instance does not expand the others.
- Filtering must show all matching appearances.

## 7. Performance Requirements

The UI must remain responsive regardless of data volume.

### Required techniques

- lazy loading
- paging of large child sets
- filtering before expansion
- no full‑tree traversal
- no client‑side recursion
- no large JavaScript files
- no DOM explosions

Shared.Tree must remain a simple renderer.  
All performance logic lives in the Subject‑specific layer.

## 8. Summary

The Subject Tree UI is a Subject‑specific wrapper around the generic Shared.Tree components.  
It must:

- support enquiries, namespace construction, and Subject maintenance  
- remain responsive for large datasets  
- support multiple roots and arbitrary user‑defined structure  
- integrate with the Namespace Selector  
- implement lazy loading and filtering  
- never modify Shared.Tree  
- follow the architectural pattern established by Admin.Manager  

## Phase 2.4.4 — Component Interaction Flow {#phase244}

This section defines how the Subject‑specific tree components interact with the generic
`Shared.Tree` components.  
This flow mirrors the architecture used by `Admin.Manager` and must be followed exactly.

The Shared.Tree components **must not be modified**.  
All Subject‑specific behaviour is implemented in the `Subject.Tree` namespace.

### 1. High‑Level Overview

The Subject Tree UI consists of two layers:

1. **Subject‑specific layer (intelligent)**
   - SubjectTreeShell.razor
   - SubjectTreeBranch.razor
   - SubjectTreeNode.cs

2. **Shared generic layer (dumb renderer)**
   - TreeShell.razor
   - TreeBranch.razor
   - TreeNode.cs

The Subject layer:

- loads data  
- applies namespace filters  
- performs lazy loading  
- manages UI modes  
- maps SubjectTreeNode → TreeNode  

The Shared layer:

- renders nodes  
- handles expand/collapse  
- raises callbacks  

### 2. Component Responsibilities and Flow

#### 2.1 SubjectTreeShell → TreeShell

`SubjectTreeShell` is the **host** and the entry point.

**Responsibilities:**

- Initialise the tree
- Apply Namespace Selector filters
- Load root nodes lazily
- Manage UI mode (Enquiry / Namespace / Subject)
- Host the Subject detail panel
- Provide callbacks for node selection
- Map SubjectTreeNode → TreeNode for Shared.TreeShell

**Flow:**

1. SubjectTreeShell loads root `SubjectTreeNode` objects.
2. It converts them into generic `TreeNode` objects.
3. It passes the generic nodes into `TreeShell`.
4. It registers callbacks for:
   - OnExpand
   - OnCollapse
   - OnSelect

`TreeShell` does not know anything about Subjects or Namespaces.

#### 2.2 TreeShell → SubjectTreeBranch

When a user expands a node:

1. `TreeShell` raises an **OnExpand** callback.
2. The callback is handled by `SubjectTreeShell`.
3. `SubjectTreeShell` delegates the load request to `SubjectTreeBranch`.

Shared.Tree never loads children itself.

#### 2.3 SubjectTreeBranch → TreeBranch

`SubjectTreeBranch` is responsible for loading children.

**Responsibilities:**

- Lazy loading
- Paging large child sets
- Applying namespace filters before loading
- Mapping SubjectTreeNode → TreeNode
- Passing generic nodes to TreeBranch

**Flow:**

1. SubjectTreeBranch receives a request to load children.
2. It queries the database (or API) for child Subjects.
3. It applies namespace filtering (if active).
4. It creates `SubjectTreeNode` objects.
5. It maps them to generic `TreeNode` objects.
6. It passes the generic nodes to `TreeBranch`.

`TreeBranch` simply renders them.

#### 2.4 SubjectTreeNode → TreeNode

`SubjectTreeNode` is the Subject‑aware node model.

**Responsibilities:**

- Hold Subject identity (via vwContactBase)
- Hold namespace path for filtering
- Expose structural / real / virtual flags
- Expose child count for lazy loading

**Mapping Rules:**

- `SubjectTreeNode.Name` → `TreeNode.Name`
- `SubjectTreeNode.HasChildren` → `TreeNode.HasChildren`
- `SubjectTreeNode.ChildCount` → `TreeNode.ChildCount`
- `SubjectTreeNode.SubjectCode` → `TreeNode.Key`
- Node type flags are mapped to TreeNode metadata (icons, CSS classes)

Shared.Tree never sees Subject‑specific fields.

### 3. Callback Flow (Selection, Expansion, Filtering)

#### 3.1 Node Selection

1. User selects a node in Shared.Tree.
2. `TreeShell` raises OnSelect.
3. `SubjectTreeShell` receives the callback.
4. `SubjectTreeShell` loads Subject details.
5. The detail panel updates.

#### 3.2 Expansion

1. User expands a node.
2. `TreeShell` raises OnExpand.
3. `SubjectTreeShell` delegates to SubjectTreeBranch.
4. SubjectTreeBranch loads children.
5. Children are mapped to TreeNode.
6. Shared.Tree renders them.

#### 3.3 Namespace Filtering

1. User types into Namespace Selector.
2. SubjectTreeShell recalculates visible roots.
3. All branches collapse.
4. Only matching nodes remain visible.
5. Lazy loading applies under the new filter.

Shared.Tree is unaware filtering is happening.

### 4. Mode Flow (Enquiry / Namespace / Subject)

`SubjectTreeShell` exposes a mode flag:

- EnquiryMode
- NamespaceMode
- SubjectMode

**Flow:**

1. Mode is set in SubjectTreeShell.
2. SubjectTreeBranch adjusts context menus.
3. SubjectTreeNode adjusts available actions.
4. Shared.Tree remains unaware of modes.

### 5. Summary

The Subject Tree UI is a **wrapper** around the generic Shared.Tree components.

- SubjectTreeShell controls the tree.
- SubjectTreeBranch loads children.
- SubjectTreeNode holds Subject data.
- Shared.Tree renders nodes and raises callbacks.

This architecture ensures:

- no modifications to Shared.Tree  
- no large JavaScript files  
- no client‑side recursion  
- full support for lazy loading, filtering, and multi‑mode behaviour  
- alignment with the Admin.Manager pattern  

## Phase 2.4.5 — Data Loading Contracts {#phase245}

This section defines the contracts for data loading between the Subject‑specific components
and the underlying data sources.  
These contracts are mandatory and must be honoured by any implementation.

All loading, filtering, and paging logic lives in the `Subject.Tree` layer.  
`Shared.Tree` never talks to the database or APIs directly.

### 1. Root loading contract

**Caller:** `SubjectTreeShell`  
**Purpose:** Load the initial set of root nodes for the tree.

**Input:**

- `NamespaceFilter` (string, may be empty)
- `Mode` (Enquiry / Namespace / Subject)
- `PageNumber` (int, default 1)
- `PageSize` (int, implementation default, e.g. 50)

**Output:**

- `IEnumerable<SubjectTreeNode> Roots`
- `int TotalRootCount`
- `bool HasMorePages`

**Rules:**

- If `NamespaceFilter` is empty → return all roots (paged).
- If `NamespaceFilter` is set → return only roots whose namespace path matches.
- Do not load children here.
- Map `SubjectTreeNode` → `TreeNode` before passing to `Shared.TreeShell`.

### 2. Child loading contract

**Caller:** `SubjectTreeBranch` (via callback from `SubjectTreeShell`)  
**Purpose:** Load children for a given parent node.

**Input:**

- `ParentSubjectCode` (string)
- `NamespaceFilter` (string, may be empty)
- `PageNumber` (int, default 1)
- `PageSize` (int, implementation default, e.g. 50)

**Output:**

- `IEnumerable<SubjectTreeNode> Children`
- `int TotalChildCount`
- `bool HasMorePages`

**Rules:**

- Apply `NamespaceFilter` before returning children.
- If `TotalChildCount` is large, rely on paging.
- Do not attempt to load grandchildren.
- Map `SubjectTreeNode` → `TreeNode` before passing to `Shared.TreeBranch`.

### 3. SubjectTreeNode shape

`SubjectTreeNode` must expose at least:

- `string SubjectCode`
- `string SubjectClassCode`
- `string Name`
- `bool HasChildren`
- `int ChildCount`
- `string NamespacePath`
- `bool IsStructural`
- `bool IsReal`
- `bool IsVirtual`

Optional (but recommended):

- `string DisplayLabel` (resolved from `vwContactBase` where applicable)

These fields are used for:

- mapping to `TreeNode`
- filtering
- lazy loading decisions
- visual differentiation (icons / styles)

### 4. Mapping contract: SubjectTreeNode → TreeNode

Before passing data into `Shared.Tree`, the Subject layer must map:

- `TreeNode.Key` ← `SubjectTreeNode.SubjectCode`
- `TreeNode.Name` ← `SubjectTreeNode.Name` or `DisplayLabel`
- `TreeNode.HasChildren` ← `SubjectTreeNode.HasChildren`
- `TreeNode.ChildCount` ← `SubjectTreeNode.ChildCount`
- `TreeNode.Metadata` (or equivalent) ← node type flags (structural / real / virtual)

`Shared.Tree` must not see Subject‑specific fields directly.

### 5. Filtering contract

Filtering is applied **only** in the Subject layer.

**Input:**

- `NamespaceFilter` (string)

**Semantics:**

- Case‑insensitive substring match on `NamespacePath`.
- May also match on `Name` / `DisplayLabel` if required.
- Applied:
      - when loading roots
      - when loading children

**Rules:**

- Filtering must not modify data or relationships.
- It only affects which nodes are returned and rendered.

### 6. Error and empty‑state handling

If a load operation returns:

- `TotalRootCount == 0` → show “No Subjects found” in the tree area.
- `TotalChildCount == 0` → show no children; do not show an expand arrow.
- Errors (connectivity, query failure, etc.) → surface a non‑blocking message in `SubjectTreeShell`; do not break `Shared.Tree`.

`Shared.Tree` must remain functional even if data loading fails.

### 7. Summary

These contracts define:

- how roots are loaded  
- how children are loaded  
- how nodes are shaped  
- how mapping to `Shared.Tree` works  
- how filtering and paging are applied  

They are mandatory for any implementation of the Subject Tree UI.

## Phase 2.5 — Namespace Selector Component Specification {#phase25}

The Namespace Selector is a reusable UI component that provides a dynamic filtering lens over
the Subject Namespace.  
It is used by the Subject Tree, by search workflows, and by any UI that needs to restrict
Subject visibility to a subset of the Namespace.

The selector must be implemented as a standalone component:

`Subject/Controls/NamespaceSelector.razor`

It must not modify or depend on Shared.Tree.

### Namespace Construction Rule (Mandatory)

The Subject Namespace is constructed from `SubjectCode`, not `SubjectName`.

- `SubjectCode` is deterministic, stable, and free of illegal characters.
- `SubjectName` is user‑entered, may contain spaces, punctuation, and illegal characters, and is not suitable for namespace paths.

Therefore:

- `NamespacePath` is always built from `SubjectCode`.
- Filtering and navigation operate on the `NamespacePath`.
- Display labels may use `SubjectName`, but the underlying namespace must not.

This ensures:

- deterministic behaviour
- reproducible synthetic datasets
- compatibility with existing SubjectCode generation
- no illegal characters in namespace paths

### 1. Purpose

The Namespace Selector provides:

- real‑time filtering of the Subject Tree  
- namespace‑aware search  
- a consistent mechanism for selecting a structural context  
- a reusable control for future workflows (e.g., project assignment, cost centre selection)

It is not a search box.  
It is a **namespace filter**.

### 2. Component Responsibilities

The selector must:

1. Accept user input (free‑text).
2. Emit filter events to its parent (e.g., SubjectTreeShell).
3. Maintain internal state (current filter string).
4. Provide debounced updates (to avoid excessive reloads).
5. Support both:
   - **Single‑select mode** (default)
   - **Multi‑select mode** (future workflows)
6. Never load Subjects directly.
7. Never query the database directly.
8. Never interact with Shared.Tree.

It is a pure UI → event emitter.

### 3. Public API (Mandatory)

The component must expose:

#### 3.1 Parameters

- `string FilterText`  
  Current filter value.

- `EventCallback<string> OnFilterChanged`  
  Raised whenever the filter changes.

- `bool MultiSelect` (default: false)  
  Reserved for future workflows.

#### 3.2 Events

- `OnFilterChanged`  
  Fired after debounce when the user types.

### 4. Behavioural Specification

#### 4.1 Real‑Time Filtering

As the user types:

- The component updates its internal `FilterText`.
- After a short debounce (e.g., 250ms), it raises `OnFilterChanged(FilterText)`.

The parent (SubjectTreeShell) is responsible for:

- recalculating visible roots  
- collapsing branches  
- triggering lazy loading under the new filter  

The selector does not manipulate the tree directly.

#### 4.2 Filter Semantics

The filter string must be interpreted as:

- case‑insensitive  
- substring match  
- applied to `NamespacePath`  
- optionally applied to `Name` / `DisplayLabel`  

The selector does not apply semantics itself — it only emits the string.

#### 4.3 Clear Behaviour

If the user clears the filter:

- `FilterText` becomes empty  
- `OnFilterChanged("")` fires  
- The tree resets to full root set (paged)

### 5. Visual Requirements

The selector must:

- appear as a simple text input  
- include a clear/reset button  
- include optional placeholder text (“Filter namespace…”)  
- avoid heavy UI chrome  
- avoid dropdowns, popovers, or complex UI elements  

This is a lightweight control.

### 6. Integration Contract

The selector must integrate with:

- `SubjectTreeShell` (mandatory)
- any future workflow requiring namespace restriction (optional)

**Flow:**

1. User types into selector.  
2. Selector raises `OnFilterChanged`.  
3. SubjectTreeShell:
   - collapses all branches  
   - reloads roots using the filter  
   - triggers lazy loading under the new filter  

Shared.Tree remains unaware of filtering.

### 7. Error Handling

The selector must not throw exceptions for:

- empty input  
- whitespace input  
- rapid typing  
- rapid clearing  

It must always emit a valid string.

### 8. Summary

The Namespace Selector is a simple, reusable, event‑driven component that:

- emits filter text  
- drives the Subject Tree  
- never loads data  
- never interacts with Shared.Tree  
- never applies semantics itself  

It is the filtering lens through which the user views the Subject Namespace.

## Phase 2.6 — Subject Detail Panel Specification {#phase26}

The Subject Detail Panel is the right‑hand contextual panel displayed when a user selects a
node in the Subject Tree.  
It provides identity, namespace, and maintenance actions for the selected Subject.

The panel is hosted exclusively by:

`Subject/Tree/SubjectTreeShell.razor`

Shared.Tree must not render or manage the detail panel.

## 1. Purpose

The detail panel provides:

- a consistent view of Subject identity  
- a summary of namespace relationships  
- access to maintenance actions (depending on mode)  
- a stable UI surface for all Subject types (Real, Virtual, Structural)  

It is the primary “inspection and action” surface for the Subject model.

## 2. Activation Flow

1. User selects a node in Shared.Tree.  
2. `TreeShell` raises `OnSelect(nodeKey)`.  
3. `SubjectTreeShell` receives the callback.  
4. `SubjectTreeShell` loads Subject details using `nodeKey` (SubjectCode).  
5. The detail panel updates with the loaded data.

Shared.Tree is unaware that a detail panel exists.

## 3. Data Loading Contract

The detail panel loads a `SubjectDetailModel` using `SubjectCode`.

The model must contain:

- `SubjectCode`
- `SubjectClassCode`
- `SubjectTypeCode`
- `Name` (primary display name)
- `DisplayLabel` (resolved identity label)
- `IsStructural`
- `IsReal`
- `IsVirtual`
- `NamespacePaths` (all parent paths in the DAG)
- `IdentityFields` (SubjectType‑specific identity fields)

### IdentityFields

`IdentityFields` is a structured object whose shape depends on `SubjectClassCode`.

The implementation may use:

- a switch statement  
- a factory  
- a SubjectType‑driven metadata map  

The model may choose the mechanism, but the behaviour must be:

- Structural → structural identity fields  
- Real → person/organisation identity fields  
- Virtual → virtual identity fields  

The model must not depend on legacy tables or views.

## 4. Display Requirements

The panel must present the following blocks:

### 4.1 Identity Block

- DisplayLabel (primary)
- SubjectCode (secondary)
- SubjectType (Real / Virtual / Structural)
- SubjectClassCode

### 4.2 Namespace Block

- All namespace paths in which the Subject appears
- Each path shown as a breadcrumb
- Clicking a breadcrumb recentres the tree on that path (optional [Phase 2.7](#phase27) behaviour)

### 4.3 IdentityFields Block (Real Subjects only)

Display the SubjectType‑specific identity fields.

### 4.4 Notes Block

- Free‑text notes
- Editable only in SubjectMode

## 5. Mode‑Dependent Behaviour

The panel must adapt to the current UI mode:

### 5.1 EnquiryMode

- All fields read‑only  
- No editing controls  
- No destructive actions  

### 5.2 NamespaceMode

- Show controls for:
      - Add structural child  
      - Reparent Subject  
      - Remove namespace relationship  
- Identity fields remain read‑only  

### 5.3 SubjectMode

- Show controls for:
      - Edit Subject (opens Razor Page)  
      - Change Subject type  
      - Delete Subject (if allowed)  
- Notes become editable  
- Identity fields editable only via Razor Page  

## 6. Subject Maintenance Razor Pages (Mandatory)

Each SubjectClassCode requires its own Razor Page for Subject maintenance.

This is a platform requirement because Razor Pages:

- behave correctly under the `IsEmbedded` flag  
- provide mobile‑compatible layouts  
- support Subject‑type‑specific UI  
- avoid Blazor component lifecycle issues  
- match the existing TCWeb architecture  

### Required Razor Pages

1. `Subject/Tree/EditReal.cshtml`  
2. `Subject/Tree/EditVirtual.cshtml`  
3. `Subject/Tree/EditStructural.cshtml`

### Behaviour

- The detail panel routes all “Edit” actions to the correct Razor Page based on `SubjectClassCode`.  
- Razor Pages load and save Subject data via `SubjectCode`.  
- Razor Pages adapt their UI to the fields defined for that SubjectClassCode.  
- Razor Pages must not depend on legacy `tbContact` or `vwContactBase`.  
- Razor Pages must use the Subject identity model and SubjectType configuration.

## 7. Action Contracts

All actions must be routed through `SubjectTreeShell`.

### 7.1 Edit Subject

Input:

- `SubjectCode`
- Updated fields (via Razor Page)

Output:

- Updated `SubjectDetailModel`
- Tree refresh if name or type changed

### 7.2 Change Subject Type

Input:

- `SubjectCode`
- New `SubjectTypeCode`

Output:

- Updated node metadata  
- Tree refresh  

### 7.3 Add Structural Child

Input:

- `ParentSubjectCode`
- New structural node details

Output:

- New node added under parent  
- TreeBranch reload  

### 7.4 Reparent Subject

Input:

- `SubjectCode`
- `NewParentSubjectCode`

Output:

- Updated namespace relationships  
- Tree refresh  

### 7.5 Delete Subject

Input:

- `SubjectCode`

Output:

- Node removed from tree  
- Panel cleared  

Shared.Tree must not perform any of these actions.

## 8. Error Handling

If detail loading fails:

- Show a non‑blocking error message in the panel.  
- Do not collapse or reset the tree.  
- Do not clear the panel unless the Subject no longer exists.  

If an action fails:

- Show an inline error message.  
- Keep the panel open.  
- Do not mutate the tree.  

## 9. Summary

The Subject Detail Panel:

- is hosted by SubjectTreeShell  
- activates on node selection  
- loads Subject details via SubjectCode  
- adapts to UI mode  
- provides identity, namespace, and maintenance actions  
- invokes Razor Pages for editing  
- never interacts with Shared.Tree  
- never performs direct data access  

It is the central inspection and action surface for the Subject model.

## Phase 2.7 — Namespace Semantics and Resolution Rules {#phase27}

The Subject Namespace is a directed acyclic graph (DAG) representing hierarchical
relationships between Subjects.  
This section defines how namespace paths are constructed, resolved, filtered, and displayed.

These rules are mandatory and must be followed by all components that interact with the
namespace, including:

- SubjectTreeShell  
- SubjectTreeBranch  
- NamespaceSelector  
- Subject maintenance Razor Pages  
- Subject services  

## 1. Namespace Identity (Mandatory Rule)

### 1.1 Namespace is constructed from `SubjectCode`

The namespace path for any Subject is constructed exclusively from `SubjectCode`.

This ensures:

- determinism  
- stability  
- no illegal characters  
- reproducibility  
- compatibility with synthetic datasets  
- compatibility with existing SubjectCode generation  

### 1.2 SubjectName must not be used in namespace paths

SubjectName:

- is user‑entered  
- may contain spaces  
- may contain punctuation  
- may contain illegal characters  
- is not stable  
- is not guaranteed unique  

Therefore, SubjectName is **display‑only**, not structural.

## 2. Namespace Path Construction

### 2.1 Path Format

Namespace paths use dot‑notation, not forward slashes.

A namespace path is a `.`‑delimited sequence of SubjectCodes:

`ROOT.CHILD.GRANDCHILD`

### 2.2 Multiple Parents

Because the namespace is a DAG:

- a Subject may appear under multiple parents  
- each appearance generates a distinct namespace path  
- all paths must be preserved  

Example:

``` text
A.B.C
X.B.C
```

Subject `C` has two valid namespace paths.

### 2.3 Path Storage

Each Subject must store:

- zero or more parent relationships  
- zero or more namespace paths (derived, not stored)  

Paths are computed on demand.

## 3. Namespace Resolution

### 3.1 Identity Resolution

Given a `SubjectCode`, the system must resolve:

- Subject identity  
- SubjectType  
- SubjectClassCode  
- all parent relationships  
- all namespace paths  

### 3.2 Display Resolution

Display labels must use:

- `DisplayLabel` (SubjectType‑specific identity)  
- or `Name` (fallback)  

Display labels must not affect namespace identity.

## 4. Filtering Semantics

Filtering is performed by the Subject layer, not Shared.Tree.

### 4.1 Filter Input

The NamespaceSelector emits a raw string:

`FilterText`

### 4.2 Filter Application

Filtering is applied to:

- `NamespacePath` (mandatory)  
- `DisplayLabel` (optional)  
- `Name` (optional)  

### 4.3 Matching Rules

- case‑insensitive  
- substring match  
- applied before lazy loading  
- applied to both roots and children  

### 4.4 Filtering Must Not Modify Data

Filtering affects visibility only.

## 5. Multi‑Parent Semantics

### 5.1 Independent Instances

Each appearance of a Subject in the namespace is treated as an independent instance in the tree.

### 5.2 Expansion Independence

Expanding one instance must not expand any other instance.

### 5.3 Filtering Independence

Filtering must show all matching instances.

## 6. Future Requirement: Human‑Readable SubjectCodes

The current 10‑character SubjectCode format is deterministic but not user‑friendly.

In a future phase, `App.proc_DefaultCodeGenerator` must be updated or replaced to generate
SubjectCodes that incorporate whole words or meaningful tokens.

This will allow:

- human‑readable namespace paths  
- intuitive navigation  
- improved filtering  
- better UX for large multi‑tenant environments  

Identity remains SubjectCode; display remains SubjectName; namespace remains code‑based.

## 7. Summary

Namespace semantics are defined by the following rules:

- Namespace paths are constructed from SubjectCode  
- SubjectName is display‑only  
- The namespace is a DAG with multi‑parent support  
- Filtering is applied to namespace paths and display labels  
- Filtering does not modify data  
- Each namespace appearance is independent  
- Future SubjectCodes will be human‑readable  

These rules ensure a deterministic, scalable, and user‑friendly namespace model.

## Phase 2.8 - Subject Maintenance {#phase28}

## Phase 2.8.1 — Workflow Maintenance {#phase281}

This section defines the end‑to‑end workflows for maintaining Subjects:

- creating new Subjects
- editing existing Subjects
- changing Subject type
- managing namespace relationships
- deleting Subjects (where allowed)

All workflows must:

- use `SubjectCode` as the identity
- respect `SubjectClassCode` and `SubjectTypeCode`
- route editing through the Subject maintenance Razor Pages
- preserve namespace DAG semantics
- avoid direct interaction with Shared.Tree

### 1. Workflow overview

The following workflows are in scope:

1. Create Structural Subject
2. Create Real Subject
3. Create Virtual Subject
4. Edit Subject (Real / Virtual / Structural)
5. Change Subject Type
6. Manage Namespace Relationships (add / reparent / remove)
7. Delete Subject

All workflows are initiated from:

- the Subject Tree
- the Subject Detail Panel
- or a future Subject search/listing surface

### 2. Create Structural Subject

**Entry points:**

- From NamespaceMode in the Subject Detail Panel:
      - “Add structural child” under a selected node
- From a root‑level action:
      - “Add structural root” (if allowed)

**Flow:**

1. User triggers “Add structural child” (or root).
2. `SubjectTreeShell` opens `Subject/Tree/EditStructural.cshtml` with:
   - `ParentSubjectCode` (optional for root)
   - `IsEmbedded` flag set appropriately.
3. Razor Page:
   - presents Structural Subject fields
   - validates input
   - creates new Structural Subject
   - creates namespace relationship to parent (if provided)
4. On success:
   - Razor Page returns `SubjectCode` of new node.
   - `SubjectTreeShell` reloads the relevant branch (or roots).
   - Detail panel selects and displays the new Subject.

**Rules:**

- Structural Subjects must not carry Real/Virtual identity fields.
- Structural Subjects may appear in multiple namespace paths later via reparenting.

### 3. Create Real Subject

**Entry points:**

- From NamespaceMode or SubjectMode:
      - “Add real child” under a selected node (if allowed by SubjectType)
- From a global “New Subject” action (future list/search UI).

**Flow:**

1. User triggers “Add real child” or “New real Subject”.
2. `SubjectTreeShell` opens `Subject/Tree/EditReal.cshtml` with:
   - optional `ParentSubjectCode`
   - `IsEmbedded` flag.
3. Razor Page:
   - presents Real Subject identity fields (SubjectType‑driven)
   - validates input
   - creates new Real Subject
   - creates namespace relationship to parent (if provided)
4. On success:
   - Razor Page returns `SubjectCode`.
   - `SubjectTreeShell` reloads branch/roots.
   - Detail panel selects and displays the new Subject.

**Rules:**

- Identity fields are defined by `SubjectTypeCode`.
- No dependency on legacy `tbContact` or `vwContactBase`.

### 4. Create Virtual Subject

**Entry points:**

- From NamespaceMode:
      - “Add virtual child” under a selected node (if allowed)
- From future workflows that require synthetic grouping nodes.

**Flow:**

1. User triggers “Add virtual child”.
2. `SubjectTreeShell` opens `Subject/Tree/EditVirtual.cshtml` with:
   - `ParentSubjectCode`
   - `IsEmbedded` flag.
3. Razor Page:
   - presents Virtual Subject fields (label, behaviour, etc.)
   - validates input
   - creates new Virtual Subject
   - creates namespace relationship to parent
4. On success:
   - Razor Page returns `SubjectCode`.
   - `SubjectTreeShell` reloads branch.
   - Detail panel selects and displays the new Subject.

**Rules:**

- Virtual Subjects must be clearly distinguishable in the tree (metadata/flags).
- Virtual Subjects may also be multi‑parent.

### 5. Edit Subject

**Entry points:**

- From SubjectMode in the detail panel:
      - “Edit Subject”

**Flow:**

1. User clicks “Edit Subject”.
2. `SubjectTreeShell` determines `SubjectClassCode` and opens:
   - Real → `Subject/Tree/EditReal.cshtml`
   - Virtual → `Subject/Tree/EditVirtual.cshtml`
   - Structural → `Subject/Tree/EditStructural.cshtml`
3. Razor Page:
   - loads Subject by `SubjectCode`
   - presents appropriate fields for that class
   - validates and saves changes
4. On success:
   - Razor Page returns updated `SubjectCode` (unchanged) and any changed display fields.
   - `SubjectTreeShell`:
     - refreshes the selected node in the tree (name/label/type)
     - reloads the detail panel.

**Rules:**

- SubjectCode must not change as part of edit.
- If SubjectType changes, see “Change Subject Type” workflow.

### 6. Change Subject Type

**Entry points:**

- From SubjectMode in the detail panel:
      - “Change Subject type”

**Flow:**

1. User triggers “Change Subject type”.
2. `SubjectTreeShell` opens an appropriate UI (dialog or Razor Page) to:
   - select new `SubjectTypeCode`
   - confirm implications (fields gained/lost).
3. Service layer:
   - validates the transition
   - migrates identity/attribute data as required
   - updates `SubjectTypeCode` and related metadata.
4. On success:
   - `SubjectTreeShell` refreshes:
     - tree node metadata (Real/Virtual/Structural flags)
     - detail panel
   - future edits route to the correct Razor Page.

**Rules:**

- Not all transitions may be allowed (e.g., Structural → Real may be restricted).
- The service layer, not the UI, enforces allowed transitions.

### 7. Manage namespace relationships

Namespace relationships are managed in **NamespaceMode**.

#### 7.1 Add to namespace (additional parent)

**Entry points:**

- From NamespaceMode:
      - “Add to namespace…” action on a Subject.

**Flow:**

1. User selects a Subject.
2. User chooses “Add to namespace…”.
3. UI presents a mechanism (tree picker or search) to select a new parent.
4. Service layer:
   - creates a new parent–child relationship.
5. On success:
   - `SubjectTreeShell` reloads the relevant branches.
   - Subject now appears under multiple paths.

#### 7.2 Reparent Subject

**Entry points:**

- From NamespaceMode:
      - “Reparent” on a specific instance of a Subject in the tree.

**Flow:**

1. User selects a node instance (path‑specific).
2. User chooses “Reparent”.
3. UI allows selection of a new parent.
4. Service layer:
   - removes the old parent–child relationship for that path
   - creates a new parent–child relationship.
5. On success:
   - `SubjectTreeShell` reloads affected branches.

#### 7.3 Remove from namespace

**Entry points:**

- From NamespaceMode:
      - “Remove from namespace” on a specific instance.

**Flow:**

1. User selects a node instance.
2. User chooses “Remove from namespace”.
3. Service layer:
   - removes that parent–child relationship.
4. On success:
   - node disappears from that path only.
   - other paths remain intact.

**Rules:**

- Removing the last namespace relationship may be disallowed or treated as “orphaned Subject” (business rule).
- All operations are path‑specific; other instances are unaffected.

### 8. Delete Subject

**Entry points:**

- From SubjectMode in the detail panel:
      - “Delete Subject” (if allowed)

**Flow:**

1. User triggers “Delete Subject”.
2. UI presents a confirmation with clear consequences.
3. Service layer:
   - validates that deletion is allowed (no blocking dependencies).
   - removes:
     - the Subject record
     - all namespace relationships
     - related attributes/identity records as per business rules.
4. On success:
   - `SubjectTreeShell`:
     - removes all instances of the Subject from the tree
     - clears the detail panel.

**Rules:**

- Deletion may be restricted to certain SubjectTypes or environments.
- Soft‑delete vs hard‑delete is a service‑layer decision, not a UI concern.

### 9. IsEmbedded behaviour

All Razor Pages must respect the `IsEmbedded` flag:

- **IsEmbedded = true**
      - Page renders without full layout chrome.
      - Navigation is delegated back to the host (SubjectTreeShell).
      - Suitable for panel‑style or modal embedding.

- **IsEmbedded = false**
      - Page behaves as a standalone full‑screen editor.
      - Suitable for mobile or direct navigation.

SubjectTreeShell must set `IsEmbedded` appropriately based on hosting context.

### 10. Summary

Subject maintenance workflows:

- are initiated from the tree and detail panel
- use `SubjectCode` as the stable identity
- route editing through three Razor Pages:
      - `Subject/Tree/EditReal.cshtml`
      - `Subject/Tree/EditVirtual.cshtml`
      - `Subject/Tree/EditStructural.cshtml`
- respect SubjectClassCode and SubjectTypeCode
- preserve namespace DAG semantics
- never allow Shared.Tree to perform data access or business logic

These workflows complete the Subject UI model for Phase 2.

### 2.8.2 — Address Management Workflows {#phase282}

Addresses are stored in a dedicated table:

    Subject.tbAddress
        AddressCode nvarchar(15) PK
        SubjectCode nvarchar(10) FK → Subject.tbSubject(SubjectCode)
        Address nvarchar(max) -- free‑form address text
        InsertedBy / InsertedOn / UpdatedBy / UpdatedOn / RowVer

The default address for a Subject is held on:

    Subject.tbSubject.AddressCode

A trigger on `Subject.tbAddress` ensures that when a new address is inserted
for a Subject whose `Subject.AddressCode` is NULL, the new `AddressCode` is
copied into `Subject.tbSubject.AddressCode`.

There is no structured address model (no postcode, country, etc.) in Phase 2.
`Address` is free‑form text.

#### 1. Display in the Subject Detail Panel

The detail panel must show:

- **Address list:**
  - all rows from `Subject.tbAddress` for the current `SubjectCode`
  - each row displaying the free‑form `Address` text
- **Default indicator:**
  - the address whose `AddressCode` = `Subject.tbSubject.AddressCode`
    must be clearly marked as the default

No parsing or formatting of `Address` is required beyond preserving line breaks.

#### 2. Add address workflow

**Entry point:**

- SubjectMode → “Add address”

**Flow:**

1. User clicks “Add address”.
2. `SubjectTreeShell` opens `Subject/Tree/EditReal.cshtml` (Real Subjects only),
   with `SubjectCode` and `IsEmbedded` as usual.
3. Razor Page:
   - presents a multiline text area for `Address`
   - validates basic rules (e.g., not empty if required)
   - inserts a new row into `Subject.tbAddress` with a new `AddressCode`.
4. Trigger behaviour:
   - if `Subject.tbSubject.AddressCode` is NULL for this Subject,
     the insert trigger sets it to the new `AddressCode` (first address becomes default).
5. On success:
   - Razor Page returns to host
   - `SubjectTreeShell` reloads the address list in the detail panel.

#### 3. Edit address workflow

**Entry point:**

- SubjectMode → “Edit” on a specific address

**Flow:**

1. User selects an address (by `AddressCode`) to edit.
2. `EditAddress.cshtml` loads the row from `Subject.tbAddress`.
3. User edits the free‑form `Address` text.
4. On save:
   - the row is updated
   - the update trigger refreshes `UpdatedBy` / `UpdatedOn`.
5. `SubjectTreeShell` reloads the address list in the detail panel.

The default flag is unaffected by editing; it is controlled solely by `Subject.tbSubject.AddressCode`.

#### 4. Delete address workflow

**Entry point:**

- SubjectMode → “Delete” on a specific address

**Flow:**

1. User triggers delete for a given `AddressCode`.
2. Service layer deletes the row from `Subject.tbAddress`.
3. If the deleted `AddressCode` equals `Subject.tbSubject.AddressCode`:
   - business rule must decide:
     - either set `Subject.tbSubject.AddressCode` to NULL, or
     - set it to another existing `AddressCode` for that Subject
   - this logic belongs in the service layer (not in a trigger).
4. `SubjectTreeShell` reloads the address list and default indicator.

#### 5. Change default address workflow

There is no `IsDefault` column on `Subject.tbAddress`.  
The default is defined by `Subject.tbSubject.AddressCode`.

**Entry point:**

- SubjectMode → “Set as default” on a non‑default address

**Flow:**

1. User selects an address row (by `AddressCode`).
2. Service layer:
   - updates `Subject.tbSubject.AddressCode` to the selected `AddressCode`.
3. `SubjectTreeShell` reloads the address list; the default indicator moves accordingly.

No changes are made to `Subject.tbAddress` rows for this operation.

#### 6. Computed postcode

A computed column added to `Subject.tbAddress`
to extract a postcode from the free‑form `Address` field, e.g.:

    ALTER TABLE Subject.tbAddress
    ADD Postcode AS dbo.fnExtractPostcode(Address) PERSISTED;

This would enable postcode‑based search and reporting without changing the UI
or the existing workflows.

#### 7. Summary

- Addresses are stored in `Subject.tbAddress` as free‑form text.
- The default address is defined by `Subject.tbSubject.AddressCode`.
- First address for a Subject becomes default via insert trigger.
- All add/edit/delete/default operations are performed via `EditReal.cshtml`
  and the Subject service layer.
- No structured address model is assumed in Phase 2.

## Phase 2.9 — Validation and Guardrails {#phase29}

This section defines the validation rules and guardrails that must be enforced across all Subject workflows, Razor Pages, and service‑layer operations. These rules ensure that the enhanced Subject model remains deterministic, consistent, and free from illegal states. Validation is performed in the service layer; the UI may provide convenience checks, but the service layer is authoritative.

### 1. Subject identity validation

Every Subject must satisfy:

- `SubjectCode` is non‑null, unique, immutable, and `nvarchar(50)`
- `SubjectTypeCode` is valid and maps to a single `(SubjectClassCode, CashPolarityCode)`
- `SubjectClassCode` is derived from `SubjectTypeCode` and must not be stored on `tbSubject`
- `Name` or equivalent identity field must not be empty
- `DisplayLabel` must resolve deterministically from identity fields

The service layer must reject:

- attempts to change `SubjectCode`
- attempts to assign an invalid `SubjectTypeCode`
- attempts to assign a `SubjectTypeCode` whose class/polarity is incompatible with the Subject’s extension table
- attempts to create a Subject without the required extension row (`tbReal`, `tbVirtual`, or `tbStructural`)

### 2. Namespace validation

Namespace operations must enforce:

- parent and child must both exist
- parent and child must not be the same Subject
- adding a parent must not create a cycle
- reparenting must not create a cycle
- removing a parent must not orphan a Subject unless explicitly allowed
- namespace paths must be constructed from `SubjectCode`
- namespace paths must not use `Name` or display labels

The service layer must reject:

- duplicate parent–child relationships
- illegal reparenting operations
- attempts to use SubjectName in namespace identity
- attempts to create a namespace relationship for a Subject that does not have the correct extension row

### 3. Address validation

Addresses are stored in `Subject.tbAddress` as free‑form text.

Validation rules:

- `Address` must not be null or empty
- `AddressCode` must be unique
- `SubjectCode` must reference an existing Subject
- deleting the default address must update or clear `tbSubject.AddressCode`
- the insert trigger must not be bypassed

The service layer must reject:

- attempts to assign a default address that does not belong to the Subject
- attempts to delete an address without resolving the default
- attempts to create an address for a Subject that is not Real (unless future rules allow otherwise)

### 4. Subject type transition validation

Changing `SubjectTypeCode` must satisfy:

- the transition is allowed by business rules
- required identity fields for the new type are present
- incompatible fields are removed or migrated
- the SubjectClassCode implied by the new type matches the existing extension table

The service layer must reject:

- transitions that would leave the Subject in an incomplete state
- transitions that require moving the Subject between extension tables (this is not supported in Phase 2)
- transitions that violate class/polarity compatibility

### 5. Razor Page guardrails

Razor Pages must:

- load data exclusively via `SubjectCode`
- never mutate Shared.Tree
- never perform direct SQL
- always call the service layer for save/delete operations
- respect the `IsEmbedded` flag
- validate user input before calling the service layer

The service layer must reject:

- malformed or incomplete payloads
- attempts to modify read‑only fields
- attempts to bypass required extension‑table fields

### 6. Tree guardrails

Shared.Tree must:

- never perform data access
- never mutate Subjects
- never mutate namespace relationships
- never infer behaviour from SubjectName
- never assume a Subject has only one parent

SubjectTreeShell is the only component allowed to:

- load Subject details
- refresh branches
- route edit actions
- apply filters
- manage UI mode

### 7. Error handling guardrails

All errors must:

- be non‑blocking
- leave the tree intact
- leave the detail panel intact
- provide a clear message to the user
- never expose SQL or internal exception details

The service layer must:

- log errors via `App.proc_ErrorLog`
- return safe, user‑friendly messages

### 8. Data integrity guardrails

The following must always hold:

- every Subject has a valid `SubjectTypeCode`
- every Subject has exactly one extension row (`tbReal`, `tbVirtual`, or `tbStructural`)
- every namespace relationship is acyclic
- every address belongs to exactly one Subject
- default address (if any) must reference an existing address row
- deleting a Subject cascades to its addresses
- deleting an address updates default address state
- namespace paths must always be derived from `SubjectCode`

### 9. Summary

Validation and guardrails ensure:

- Subjects remain structurally valid
- namespace relationships remain acyclic and deterministic
- Razor Pages cannot corrupt the model
- Shared.Tree remains a pure renderer
- address handling remains consistent
- Subject type transitions remain safe
- errors never destabilise the UI

These rules complete the defensive architecture for Phase 2.

## Phase 2.10 — Component Responsibilities and Boundaries {#phase210}

This section defines the strict architectural boundaries between the Subject‑specific components, the Shared.Tree components, the service layer, and the Razor Pages. These boundaries prevent architectural drift, ensure deterministic behaviour, and guarantee that the enhanced Subject model remains stable as the UI evolves.

All components must operate only within their defined responsibilities. No component may assume or perform the responsibilities of another.

### 1. Shared.Tree Components (Generic Layer)

The Shared.Tree components are generic, reusable, and must not be modified.

Components:

- `Shared/Tree/TreeShell.razor`
- `Shared/Tree/TreeBranch.razor`
- `Shared/Tree/TreeNode.cs`

Responsibilities:

- render tree nodes
- handle expand/collapse UI interactions
- raise callbacks (`OnExpand`, `OnCollapse`, `OnSelect`)
- maintain generic tree state

Prohibited:

- no data loading
- no filtering
- no namespace logic
- no Subject‑specific behaviour
- no direct database or service calls
- no knowledge of SubjectClassCode or SubjectTypeCode

### 2. Subject.Tree Components (Subject‑Specific Layer)

Components:

- `Subject/Tree/SubjectTreeShell.razor`
- `Subject/Tree/SubjectTreeBranch.razor`
- `Subject/Tree/SubjectTreeNode.cs`

Responsibilities:

- load Subject data
- apply namespace filtering
- implement lazy loading and paging
- manage UI mode (Enquiry / Namespace / Subject)
- map `SubjectTreeNode` → `TreeNode`
- host the Subject Detail Panel
- route actions to Razor Pages or service layer

Prohibited:

- no modification of Shared.Tree components
- no direct SQL
- no assumptions about SubjectName in namespace identity
- no full‑tree loading
- no client‑side recursion

### 3. Namespace Selector

Component:

- `Subject/Controls/NamespaceSelector.razor`

Responsibilities:

- accept user input
- emit filter text via `OnFilterChanged`
- debounce input
- maintain internal filter state

Prohibited:

- no data loading
- no namespace resolution
- no interaction with Shared.Tree
- no Subject logic

### 4. Razor Pages (Subject Maintenance)

Pages:

- `Subject/Tree/EditReal.cshtml`
- `Subject/Tree/EditVirtual.cshtml`
- `Subject/Tree/EditStructural.cshtml`

Responsibilities:

- load Subject by `SubjectCode`
- present SubjectClass‑specific fields
- validate and save changes
- create new Subjects
- change SubjectType
- update identity fields
- respect `IsEmbedded` flag

Prohibited:

- no namespace filtering
- no tree rendering
- no Shared.Tree interaction
- no direct SQL (must use service layer)

### 5. Service Layer

Responsibilities:

- validate all Subject operations
- enforce composite semantics
- enforce namespace DAG rules
- enforce Subject type transition rules
- enforce address rules
- perform all data mutations
- log errors via `App.proc_ErrorLog`
- return safe, user‑friendly messages

Prohibited:

- no UI logic
- no rendering
- no assumptions about tree state
- no assumptions about filtering

### 6. Data Access Layer

Responsibilities:

- execute SQL queries
- return DTOs or models to the service layer
- enforce deterministic identity rules
- ensure correct joins for composite semantics

Prohibited:

- no UI logic
- no filtering logic
- no namespace path construction (service layer responsibility)

### 7. Component Interaction Boundaries

#### 7.1 Shared.Tree → Subject.Tree

Allowed:

- callbacks (`OnExpand`, `OnSelect`, `OnCollapse`)

Not allowed:

- Shared.Tree must not request data directly
- Shared.Tree must not apply filters

#### 7.2 Subject.Tree → Service Layer

Allowed:

- load Subject details
- load children
- load roots
- perform Subject actions (edit, delete, reparent, etc.)

Not allowed:

- direct SQL
- bypassing validation

#### 7.3 Razor Pages → Service Layer

Allowed:

- load Subject by `SubjectCode`
- save Subject changes
- validate transitions

Not allowed:

- namespace filtering
- tree manipulation

### 8. Summary

Component boundaries ensure:

- Shared.Tree remains a pure renderer
- Subject.Tree handles all Subject‑specific logic
- Razor Pages handle Subject maintenance
- the service layer enforces all rules and performs all mutations
- the Namespace Selector is a pure event emitter
- no component leaks into another’s responsibility

These boundaries complete the architectural foundation required for Phase 3.

# 3. Semantics {#semantics-overview}

## Namespace Operations and Advanced Behaviour

**Semantics** introduces the behavioural intelligence of the enhanced Subject model.  
Where Phase 1 established identity and structure, and Phase 2 established UI and workflow integration, **Semantics** defines how Subjects behave within the Namespace as a directed acyclic graph (DAG).

This phase covers:

- advanced namespace operations  
- multi‑parent semantics  
- structural editing rules  
- Subject behaviour within the DAG  
- namespace path resolution  
- filtering and search semantics  
- validation of structural changes  
- performance and consistency guarantees  

### Scope of Semantics

**Semantics** defines the complete behavioural layer of the enhanced Subject model.  
It introduces the rules, operations, and constraints that govern how Subjects participate in the Namespace, how structural changes are validated, and how the UI and service layer must coordinate to maintain a consistent DAG.

Subsections of **Semantics** will specify:

- parent/child relationship rules  
- namespace modification operations  
- DAG validation  
- namespace path construction  
- filtering and search semantics  
- performance and consistency requirements  
- error handling rules  

**Semantics** builds on the foundations of Phase 1 and Phase 2 and must not contradict any identity, schema, or UI boundary rules defined earlier.

## Semantics 3.1 — Namespace Operations Overview {#semantics31}

Semantics 1 introduces the conceptual overview of namespace operations.  
It explains how Subjects participate in the Namespace, how structural relationships are represented, and how the system must interpret and validate these relationships.

Namespace operations allow users to modify the structural relationships between Subjects.  
These operations must be deterministic, validated, and consistent with the identity and class rules defined in earlier phases.

Namespace operations include:

- adding a parent to a Subject  
- removing a parent from a Subject  
- reparenting a Subject  
- validating structural changes  
- enforcing DAG constraints  
- updating namespace paths  
- ensuring consistency across UI and service layers  

All namespace operations are performed through the service layer.  
The UI may initiate operations, but it must never apply structural changes directly.

### Goals of Namespace Operations

Namespace operations must:

- maintain a valid DAG (no cycles)  
- preserve Subject identity and class semantics  
- ensure that every structural change is validated  
- support multi‑parent Subjects  
- ensure namespace paths remain correct and deterministic  
- integrate cleanly with filtering and search  
- maintain performance for large trees  

### Scope of Semantics 1

This subsection provides:

- a conceptual overview of namespace behaviour  
- the rules governing parent/child relationships  
- the constraints that must be enforced  
- the responsibilities of each component during namespace operations  

Subsequent sections will define:

- detailed validation rules  
- service‑layer contracts  
- UI workflows  
- error handling  
- performance considerations  
- namespace path resolution  
- filtering and search semantics  

**Semantics 1** establishes the conceptual foundation for all namespace‑related behaviour in the enhanced Subject model.

## Semantics 3.2 — Parent/Child Relationship Rules {#semantics32}

Parent/child relationships define the structural position of a Subject within the Namespace DAG.  
These rules ensure that all structural relationships remain valid, deterministic, and consistent with the identity and class semantics defined in earlier phases.

A parent/child relationship is represented as a row in `Subject.tbSubjectNamespace`, where:

- `ParentSubjectCode` references the parent  
- `ChildSubjectCode` references the child  

A Subject may have multiple parents.  
A Subject may appear in multiple branches.  
The Namespace is a directed acyclic graph (DAG), not a hierarchy.

### Core Relationship Rules

The following rules apply to all parent/child relationships:

- both parent and child must exist  
- parent and child must not be the same Subject  
- adding a parent must not create a cycle  
- removing a parent must not orphan a Subject unless explicitly allowed  
- a Subject may have zero, one, or many parents  
- a Subject may have zero, one, or many children  
- relationships must be stored using `SubjectCode` (never `Name`)  
- relationships must be validated by the service layer before being persisted  

### Class and Type Compatibility

Parent/child relationships must respect SubjectClass and SubjectType semantics:

- Structural Subjects may parent any class  
- Real Subjects may parent Real or Virtual Subjects  
- Virtual Subjects may parent only Virtual Subjects unless explicitly permitted  
- incompatible class combinations must be rejected  
- SubjectType transitions must not invalidate existing relationships  

The service layer is responsible for enforcing these rules.

### Behavioural Expectations

Parent/child relationships must behave consistently across the platform:

- adding a parent immediately affects namespace path resolution  
- removing a parent may reduce the number of visible paths  
- reparenting must be treated as an atomic remove‑then‑add operation  
- UI components must refresh only the affected branches  
- Shared.Tree must remain unaware of class/type semantics  

### Validation Requirements

Before any relationship is created, modified, or removed, the service layer must validate:

- existence of both Subjects  
- class/type compatibility  
- absence of cycles  
- absence of duplicate relationships  
- compliance with business rules  
- compliance with namespace semantics  

If validation fails, the operation must be rejected with a safe, user‑friendly message.

### Summary

Parent/child relationships form the backbone of the Namespace DAG.  
These rules ensure that structural changes remain valid, deterministic, and consistent with the identity and class semantics defined in earlier phases.  
They also ensure that namespace operations integrate cleanly with filtering, search, and UI behaviour throughout **Semantics**.

## Semantics 3.3 — Namespace Modification Operations {#semantics33}

Namespace modification operations allow users to change the structural relationships between Subjects within the Namespace DAG.  
All operations must be deterministic, validated, and performed exclusively through the service layer.  
The UI may initiate operations, but it must never apply structural changes directly.

Namespace modification operations include:

- adding a parent  
- removing a parent  
- reparenting a Subject  
- validating structural changes  
- enforcing DAG constraints  
- updating namespace paths  

### 1. Add Parent Operation

**Purpose:** Attach an existing Subject to a new parent.

**Input:**

- `ChildSubjectCode`
- `ParentSubjectCode`

**Validation:**

- both Subjects must exist  
- parent and child must not be the same  
- relationship must not already exist  
- class/type compatibility must be satisfied  
- operation must not create a cycle  

**Behaviour:**

- service layer inserts a row into `Subject.tbNamespace`  
- namespace paths for the child and its descendants update accordingly  
- UI refreshes only the affected branches  

### 2. Remove Parent Operation

**Purpose:** Detach a Subject from one of its parents.

**Input:**

- `ChildSubjectCode`
- `ParentSubjectCode`

**Validation:**

- relationship must exist  
- removing the parent must not violate business rules  
- removing the parent must not create an illegal orphan unless explicitly allowed  

**Behaviour:**

- service layer deletes the row from `Subject.tbNamespace`  
- namespace paths update accordingly  
- UI refreshes affected branches  

### 3. Reparent Operation

**Purpose:** Move a Subject from one parent to another.

**Input:**

- `ChildSubjectCode`
- `OldParentSubjectCode`
- `NewParentSubjectCode`

**Validation:**

- equivalent to remove + add  
- must not create a cycle  
- class/type compatibility must be satisfied  
- Subject must remain valid under the new parent  

**Behaviour:**

- service layer performs remove + add atomically  
- namespace paths update  
- UI refreshes affected branches  

### 4. Atomicity Requirements

All namespace modifications must be atomic:

- either the entire operation succeeds  
- or no changes are applied  

Partial updates are not permitted.

### 5. UI Responsibilities

The UI must:

- initiate operations via SubjectTreeShell  
- never modify `Subject.tbNamespace` directly  
- refresh only affected branches  
- surface validation errors without collapsing the tree  

### 6. Service Layer Responsibilities

The service layer must:

- validate all operations  
- enforce DAG constraints  
- enforce class/type compatibility  
- update namespace paths  
- log errors via `App.proc_ErrorLog`  
- return safe, user‑friendly messages  

### 7. Summary

Namespace modification operations allow users to reshape the Namespace DAG safely and deterministically.  
All operations must be validated, atomic, and performed exclusively through the service layer.

## Semantics 3.4 — DAG Validation Rules {#semantics34}

The Subject Namespace is a directed acyclic graph (DAG).  
To maintain structural integrity, all namespace operations must enforce strict validation rules that prevent illegal states.

### 1. Cycle Prevention

A cycle occurs when a Subject becomes its own ancestor.

**Rules:**

- adding a parent must not create a cycle  
- reparenting must not create a cycle  
- cycle detection must be performed before any structural change  

**Service layer must reject:**

- any operation that introduces a cycle  
- any operation that indirectly creates a cycle through descendants  

### 2. Duplicate Relationship Prevention

Duplicate parent/child relationships are not permitted.

**Rules:**

- `(ParentSubjectCode, ChildSubjectCode)` must be unique  
- attempts to create duplicates must be rejected  

### 3. Class and Type Compatibility

Parent/child relationships must respect SubjectClass and SubjectType semantics.

**Rules:**

- Structural Subjects may parent any class  
- Real Subjects may parent Real or Virtual Subjects  
- Virtual Subjects may parent only Virtual Subjects unless explicitly allowed  
- incompatible combinations must be rejected  

### 4. Orphaning Rules

Removing a parent must not leave a Subject in an illegal state.

**Rules:**

- a Subject may have zero parents (root)  
- removing a parent is allowed only if:  
      - the Subject has other parents, or  
      - the business rules allow root‑level Subjects  

### 5. Illegal Ancestry Rules

Some Subject types may not appear under certain ancestors.

Examples (illustrative, not exhaustive):

- a Real Subject may not appear under a Virtual Subject if business rules forbid it  
- a Structural Subject may not appear under a Real Subject  

These rules must be enforced by the service layer.

### 6. Path Consistency

Namespace paths must remain consistent after any structural change.

**Rules:**

- all paths must be recomputed after add/remove/reparent  
- no path may contain illegal characters  
- no path may contain SubjectName  
- paths must be deterministic and reproducible  

### 7. Validation Order

Validation must occur in the following order:

1. existence checks  
2. duplicate relationship checks  
3. class/type compatibility  
4. cycle detection  
5. illegal ancestry rules  
6. orphaning rules  

If any validation step fails, the operation must be rejected.

### 8. Summary

DAG validation rules ensure that the Namespace remains structurally sound, deterministic, and consistent with business semantics.  
These rules prevent cycles, illegal ancestry, incompatible relationships, and invalid orphaning, ensuring that all namespace operations preserve the integrity of the Subject model.

## Semantics 3.5 — Namespace Path Resolution {#semantics35}

Namespace paths provide a deterministic textual representation of a Subject’s position within the Namespace DAG.  
Paths are used for filtering, search, UI display, and internal consistency checks.  
They must be reproducible, stable, and independent of SubjectName.

A namespace path is constructed from the ordered sequence of `SubjectCode` values from the root to the current Subject.

Example (illustrative only):  
`ROOT.CHILD.GRANDCHILD`

### 1. Path Construction Rules

Paths must follow these rules:

- paths consist only of `SubjectCode` segments  
- segments are ordered from root → leaf  
- segments are separated by a **dot (`.`)**  
- no segment may contain `SubjectName`  
- no segment may contain illegal characters  
- paths must be deterministic and reproducible  

### 2. Multi‑Parent Path Behaviour

A Subject with multiple parents has multiple valid paths.

Rules:

- each parent produces a distinct path  
- all paths must be generated  
- filtering must match against **all** paths  
- UI may display one or more paths depending on context  

### 3. Path Caching

To maintain performance:

- paths may be cached at the service layer  
- caches must be invalidated when:  
      - a parent is added  
      - a parent is removed  
      - a Subject is reparented  
      - a SubjectCode changes  

### 4. Path Invalidation Rules

When a structural change occurs:

- the affected Subject’s paths must be recomputed  
- all descendants’ paths must be recomputed  
- unaffected branches must not be recomputed  

### 5. UI Responsibilities

The UI must:

- display paths using dot‑notation  
- never construct paths itself  
- request updated paths after structural changes  
- use paths for filtering and search  

### 6. Service Layer Responsibilities

The service layer must:

- construct all namespace paths  
- validate path correctness  
- maintain path caches  
- invalidate caches deterministically  
- return paths in a consistent dot‑notation format  

### 7. Summary

Namespace path resolution provides a deterministic representation of structural position within the DAG.  
Paths must be stable, reproducible, and updated automatically when structural changes occur.

## Semantics 3.6 — Filtering and Search Semantics {#semantics36}

Filtering and search allow users to locate Subjects within the Namespace quickly and efficiently.  
Filtering operates on **namespace paths**, which use dot‑notation.  
Search operates on Subject identity fields.

### 1. Namespace Filtering Semantics

Filtering applies to namespace paths of the form:

`A01.B14.C07`

Filtering is **namespace‑aware**, not free‑text.

Rules:

- filtering is case‑insensitive  
- filtering matches against **dot‑separated segments**  
- filtering supports:  
      - prefix matching (`A01.` behaves like `System.` in .NET)  
      - infix matching (`.B14.` matches any branch containing B14)  
      - multi‑segment matching (`A01.B14`)  
- filtering must not collapse the tree structure  
- filtering must highlight matching branches without altering relationships  

### 2. Dot‑Notation Filtering Behaviour

Dot‑notation filtering behaves like .NET namespace filtering:

- typing `A01.` restricts results to Subjects **within** the A01 subtree  
- typing `A01.B14.` restricts results to Subjects **within** that deeper subtree  
- typing `B14` matches any segment containing `B14`  
- typing `A01.B14` matches any path containing that ordered sequence  

This ensures predictable, hierarchical filtering.

### 3. Search Semantics

Search operates on Subject identity fields:

- exact or prefix match on `SubjectCode`  
- optional match on `DisplayLabel` or `Name`  
- search results must be deterministic  
- search must not modify tree state  

### 4. Performance Requirements

Filtering and search must:

- operate without full‑tree loading  
- use lazy loading  
- avoid client‑side recursion  
- avoid recomputing paths unnecessarily  
- remain performant for large datasets  

### 5. UI Responsibilities

The UI must:

- debounce filter input  
- apply filters only after user pauses typing  
- highlight matching nodes  
- preserve expansion state where possible  
- avoid collapsing unrelated branches  

### 6. Service Layer Responsibilities

The service layer must:

- provide efficient filtering endpoints  
- provide efficient search endpoints  
- return only the necessary nodes  
- avoid returning full trees  
- ensure results are consistent with namespace semantics  

### 7. Error Handling

Filtering and search must fail safely:

- invalid input must not break the UI  
- empty filters must restore the full root set  
- service errors must be surfaced without collapsing the tree  

### 8. Summary

Filtering and search provide fast, deterministic navigation of the Namespace.  
Filtering operates on **dot‑notation namespace paths**; search operates on identity.  
Both must be performant, stable, and consistent with the DAG structure.

## Semantics 3.7 — UI Workflows for Namespace Editing {#semantics37}

Namespace editing workflows define how users initiate and complete structural changes within the Namespace DAG.  
The UI must provide a clear, predictable, and non-destructive experience, while delegating all structural logic to the service layer.

Namespace editing includes:

- adding a parent  
- removing a parent  
- reparenting a Subject  
- navigating to affected nodes  
- refreshing affected branches  

All workflows must preserve tree stability and avoid collapsing unrelated branches.

### 1. Initiating Namespace Operations

Namespace operations may be initiated from:

- the Subject Detail Panel  
- context menus on tree nodes  
- toolbar actions (optional)  

The UI must:

- identify the selected Subject  
- present only valid operations  
- prevent illegal or unsupported actions  
- never construct or modify namespace paths directly  

### 2. Add Parent Workflow

Steps:

1. User selects a Subject.  
2. User chooses **Add Parent**.  
3. UI presents a selector of valid parent candidates.  
4. User selects a parent.  
5. UI sends request to service layer.  
6. On success:  
   - refresh only affected branches  
   - highlight the new parent-child relationship  
7. On failure:  
   - display validation message  
   - preserve tree state  

### 3. Remove Parent Workflow

Steps:

1. User selects a Subject.  
2. User chooses **Remove Parent**.  
3. UI confirms the action.  
4. UI sends request to service layer.  
5. On success:  
   - refresh affected branches  
   - ensure Subject remains visible if other parents exist  
6. On failure:  
   - display validation message  
   - preserve tree state  

### 4. Reparent Workflow

Steps:

1. User selects a Subject.  
2. User chooses **Reparent**.  
3. UI displays current parent and valid new parents.  
4. User selects new parent.  
5. UI sends atomic reparent request.  
6. On success:  
   - refresh affected branches  
   - highlight new location  
7. On failure:  
   - display validation message  
   - preserve tree state  

### 5. Tree Refresh Rules

The UI must refresh:

- the affected Subject  
- its ancestors  
- its descendants  

The UI must **not** refresh:

- unrelated branches  
- the entire tree  

### 6. Error Handling

Errors must:

- be displayed non-modally  
- preserve expansion state  
- never collapse the tree  
- never leave the UI in an inconsistent state  

### 7. Summary

UI workflows must provide a stable, predictable editing experience.  
All structural logic resides in the service layer; the UI is responsible only for initiating operations and refreshing affected branches.

## Semantics 3.8 — Performance and Consistency Guarantees {#semantics38}

Namespace operations must remain performant and consistent, even for large datasets.  
The system must avoid full-tree operations, unnecessary recomputation, and client-side recursion.

### 1. Lazy Loading Requirements

The tree must load:

- root nodes initially  
- children only when expanded  
- deeper levels only when required  

Lazy loading ensures:

- minimal initial payload  
- predictable performance  
- reduced server load  

### 2. Incremental Refresh Rules

After structural changes:

- only affected branches must be refreshed  
- unaffected branches must remain untouched  
- UI must not reload the entire tree  

### 3. Path Computation Efficiency

Path computation must:

- avoid recomputing unaffected paths  
- use cached paths where valid  
- invalidate caches only when necessary  
- compute descendant paths efficiently  

### 4. Filtering and Search Performance

Filtering and search must:

- operate without full-tree loading  
- use indexed lookups where possible  
- avoid scanning entire path sets  
- return only necessary nodes  

### 5. Service Layer Consistency Guarantees

The service layer must ensure:

- atomicity of namespace operations  
- deterministic path generation  
- consistent validation rules  
- no partial updates  
- no inconsistent states  

### 6. UI Consistency Guarantees

The UI must ensure:

- stable expansion state  
- predictable refresh behaviour  
- no flicker or collapse during updates  
- consistent highlighting of affected nodes  

### 7. Error Isolation

Errors must:

- be isolated to the failing operation  
- never corrupt UI state  
- never propagate inconsistent data  
- be logged via `App.proc_ErrorLog`  

### 8. Summary

Performance and consistency guarantees ensure that namespace operations remain fast, stable, and predictable.  
Lazy loading, incremental refresh, and deterministic path handling allow the system to scale without compromising user experience.

## Semantics 3.9 — Error Handling for Namespace Operations {#semantics39}

Namespace operations must fail safely, predictably, and without compromising UI stability or Namespace integrity.  
All errors must be surfaced in a controlled manner and must never leave the system in a partially updated or inconsistent state.

### 1. Error Isolation

Errors must be isolated to the specific operation that failed.

Rules:

- no partial updates  
- no side effects on unrelated branches  
- no corruption of namespace paths  
- no inconsistent parent/child relationships  

If an operation fails, the Namespace must remain exactly as it was before the attempt.

### 2. UI Error Handling

The UI must:

- display validation errors non‑modally  
- preserve tree expansion state  
- avoid collapsing the tree  
- highlight the affected Subject where appropriate  
- never display raw exception text  

Errors must be presented as safe, user‑friendly messages.

### 3. Service Layer Error Handling

The service layer must:

- validate all inputs  
- reject invalid operations with clear messages  
- log errors via `App.proc_ErrorLog`  
- never return stack traces or internal details  
- ensure atomicity of all namespace modifications  

### 4. Validation Error Types

Common validation errors include:

- cycle creation  
- duplicate parent/child relationships  
- illegal ancestry  
- incompatible class/type combinations  
- orphaning violations  
- invalid SubjectCodes  
- attempts to modify non‑existent Subjects  

Each error type must have a clear, deterministic message.

### 5. Communication Between UI and Service Layer

The service layer must return:

- a success flag  
- a safe message  
- updated paths (if applicable)  
- updated tree fragments (if applicable)  

The UI must interpret these without guessing or reconstructing logic.

### 6. Recovery Behaviour

After an error:

- the UI must remain stable  
- the tree must remain unchanged  
- the user must be able to retry the operation  
- no refresh should occur unless explicitly required  

### 7. Summary

Error handling ensures Namespace operations remain safe, predictable, and non‑destructive.  
All failures must be isolated, user‑friendly, and must never compromise the structural integrity of the DAG.

## Semantics 3.10 — Summary {#semantics310}

**Semantics** introduces the behavioural intelligence of the enhanced Subject model.  
It defines how Subjects participate in the Namespace DAG, how structural changes are validated, and how the UI and service layer coordinate to maintain consistency.

### Key Outcomes of Semantics

**Semantics** delivers:

- deterministic namespace operations  
- multi‑parent DAG semantics  
- strict validation rules  
- dot‑notation namespace paths  
- namespace‑aware filtering  
- efficient search behaviour  
- stable UI workflows for editing  
- performance and consistency guarantees  
- safe, isolated error handling  

### Relationship to Earlier Phases

- **Phase 1** established identity, schema, and SubjectCode semantics.  
- **Phase 2** established UI workflows, guardrails, and component boundaries.  
- **Semantics ** completes the model by defining behaviour, structure, and intelligence.

Together, these phases form a coherent, deterministic architecture for the enhanced Subject model.

### Completion Criteria

**Semantics** is complete when:

- all namespace operations are implemented  
- all validation rules are enforced  
- dot‑notation paths are generated and cached  
- filtering and search behave consistently  
- UI workflows are stable and predictable  
- error handling is safe and isolated  
- performance guarantees are met  

### Final Statement

With **Semantics** complete, the enhanced Subject model is fully defined.  
The system now supports a robust, multi‑parent Namespace DAG with deterministic behaviour, consistent UI integration, and strong validation guarantees.
