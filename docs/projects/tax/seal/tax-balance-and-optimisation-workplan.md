# Tax balance and optimisation work plan

**Status:** design and delivery plan
**Owner:** Trade Control open-source tax subsystem
**Accreditation dependency:** Stage 1
**MIS Mode dependency:** Stage 2

## 1. Objective

Establish a durable, fraud-resistant continuity boundary for VAT and Corporation Tax before accreditation, then use that foundation to optimise long-running and real-time tax calculations without sacrificing a complete reconstruction path.

The statutory submission and the tax statement are related but different records:

- the **submission seal** proves the exact return or accounts payload that was reviewed, approved and sent, together with the authority outcome;
- the **tax balance checkpoint** records the statement continuity needed by accounting calculations, including opening liability, movements, adjustments, payments and closing liability.

Payments do not normally form part of the submitted tax payload, but they do form part of the tax statement and its carried balance. A submission receipt is therefore not a sufficient calculation root.

## 2. Invariants

1. Sealed evidence is append-only. Corrections and supersessions create new linked evidence and never rewrite an accepted record.
2. Every non-zero tax adjustment retains the owner's explanation in both the reviewed evidence and the resulting checkpoint.
3. A checkpoint is activated only at an explicit accounting boundary and identifies its predecessor.
4. The previous checkpoint closing balance must equal the next checkpoint opening balance.
5. Full-history reconstruction remains available as an independent correctness and tamper-detection oracle.
6. Checkpoint validation fails closed on stale source data, broken chains, mismatched seals, unexplained adjustments or incompatible calculation versions.
7. The evidence and checkpoint formats remain portable across SQL Server, PostgreSQL, hosted and independently hosted deployments.

## 3. Stage 1 — accreditation seal and balance continuity

Stage 1 is completed before VAT or Corporation Tax accreditation evidence is finalised and before production submission is enabled. It does not replace the existing calculation root.

### 3.1 Seal extension

For each VAT or Corporation Tax period, seal:

- the statement opening tax balance;
- the period's calculated tax liability;
- the exact return-level adjustment and mandatory explanation;
- tax payments and repayments included by the statement boundary;
- the statement closing tax balance;
- the exact statutory payload separately approved for submission;
- the source boundary, tenant, reporting subject, tax registration, period, environment and calculation version; and
- predecessor, correction and supersession references.

The manifest must distinguish statement values from authority payload values so a verifier cannot mistake payment continuity for part of a statutory return.

### 3.2 Tax balance store

Introduce a portable database contract, provisionally `Cash.tbTaxBalance`, containing an append-only mirror of each activated statement checkpoint. The detailed schema requires implementation review, but each record must identify at least:

- tax type and reporting profile;
- reporting period and statement boundary;
- opening balance, tax due, adjustments, payments or repayments, and closing balance;
- the related submission seal and predecessor checkpoint;
- accounting-event/source boundary and protected-source digest;
- calculation and schema versions;
- creation and validation timestamps in UTC, plus business time-zone identity;
- stable actor identity and captured display name; and
- activation, invalidation and supersession state expressed through append-only events.

The table is a calculation snapshot, not the sole audit record. Its values must be reproducible from protected source data and independently checked against the signed seal package.

### 3.3 Period-close validation

Period close must:

1. load the preceding validated checkpoint;
2. reconstruct the closing statement from protected source facts through the new boundary;
3. prove that predecessor closing balance equals current opening balance;
4. reconcile the statutory component with the applicable submission seal;
5. incorporate statement payments, repayments and explained adjustments;
6. compare the candidate checkpoint with an independent full-history reconstruction;
7. write and sign the new append-only evidence only when every check passes; and
8. block close, approval or dispatch when a mismatch is found.

An operational diagnostic must distinguish an accounting mismatch, a stale candidate, a broken evidence chain and an unavailable external evidence store without exposing taxpayer data.

### 3.4 Stage 1 evidence and exit gate

- VAT and Corporation Tax statement fixtures cover opening balances, payments, repayments, adjustments and corrections.
- Repeated preparation of unchanged data produces byte-identical canonical evidence.
- One-bit mutation, backdated change, broken predecessor and cross-tenant substitution fail closed.
- Period close detects a deliberate mismatch between source reconstruction, submission seal and tax balance.
- Backup restore behind or ahead of retained evidence is detected and has a documented recovery path.
- A standalone verifier proves the package without the running application or a particular database/cloud provider.
- Accreditation environments continue to calculate from the existing full-history root.

## 4. Stage 2 — real-time tax calculation optimisation

Stage 2 belongs to the MIS Mode development cycle and begins only after Stage 1 is stable. It is not required for VAT or Corporation Tax accreditation.

### 4.1 Candidate calculations

Measure before changing any root. Prioritise calculations that repeatedly traverse the complete history, including tax-liability roll-forwards and the tax branches contributing to `Cash.vwStatementWhatIf`. Do not alter period-bounded calculations that already meet their performance target.

The candidate model is:

`Current result = latest validated opening checkpoint + accounting effects after its boundary`

### 4.2 Proof before activation

For each calculation independently:

- define the smallest sufficient restart state;
- define inclusive/exclusive event and time boundaries;
- specify correction, supersession and invalidation behaviour;
- run checkpoint and genesis calculations side by side over representative long histories;
- prove exact accounting equivalence, including cash polarity and rounding;
- benchmark latency, reads, CPU and concurrency on production-shaped tiers;
- retain a feature switch and automatic fallback to genesis; and
- activate only after reviewed migration, rollback and corruption-recovery evidence exists.

There is no universal checkpoint assumption. VAT, Corporation Tax, cash, subject balances and forward-looking what-if projections may require different sufficient state and may be activated at different times.

### 4.3 Stage 2 exit gate

- Long-history and forward-looking fixtures reproduce genesis results exactly.
- Backdated corrections invalidate or supersede every affected checkpoint deterministically.
- Rebuild and disaster recovery can reconstruct checkpoints from genesis plus retained seals.
- Measured performance improvement is material on the intended production tier.
- PostgreSQL portability is demonstrated at the contract level before SQL Server-specific acceleration is accepted.
- Independent review approves each calculation-root change; completing Stage 1 does not imply Stage 2 approval.

## 5. Explicit exclusions

- Azure SQL Ledger or another proprietary database-integrity feature is not part of validity.
- Immutable object storage may protect hosted evidence but is a deployment control, not the protocol.
- Historical source transactions are not deleted merely because a checkpoint exists.
- Accreditation is not delayed for Stage 2 performance optimisation once Stage 1 and the statutory workflows satisfy their gates.
