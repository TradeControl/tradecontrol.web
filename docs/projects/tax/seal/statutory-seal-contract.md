# Statutory Seal Contract

**Status:** Open-source architecture baseline for staged implementation
**First proving authority:** HMRC VAT sandbox
**Applies later to:** Corporation Tax and Companies House
**Does not authorise:** an external submission, production credential, immutable-retention lock, or calculation-root change

## 1. Purpose

Trade Control must be able to prove which accounting facts a user reviewed, which exact bytes were approved and sent, what the authority returned, and whether relevant history changed afterward.

The seal is a portable statutory-evidence protocol. It must remain verifiable without Azure SQL, SQL Server, PostgreSQL, the running Trade Control application, or a particular cloud provider. Azure services may protect copies of the evidence but do not define its validity.

The seal and the tax balance checkpoint are distinct but linked records. The seal proves reviewed statutory evidence and authority activity. The checkpoint records statement continuity, including payments that are not part of a tax-return payload. Accounts Mode continues to calculate from its existing roots during Stage 1. MIS Mode may later activate individually proven checkpoints under [the tax balance and optimisation work plan](tax-balance-and-optimisation-workplan.md).

## 2. Accounting and legal interpretation

A filed period is controlled, not made historically invisible. Ordinary in-place changes to protected source facts are rejected after sealing. A lawful correction, amended return, or replacement set of accounts creates new evidence and explicitly supersedes earlier evidence; it never overwrites or deletes the original.

The existing VAT and business-tax period adjustments are explicit return-level reconciliations, not rewrites of source transactions. Before submission an authorised owner may replace an adjustment, but every non-zero value requires a plain-language explanation. The exact adjustment and explanation form part of the reviewed and sealed evidence. After submission they are read-only; any later correction is recorded against an open return or through a linked statutory amendment.

Official guidance confirms that submitted VAT errors can require recorded corrections, Company Tax Returns can be amended, and amended Companies House accounts do not remove the original filing:

- <https://www.gov.uk/submit-vat-return/correct-errors-in-your-vat-return>
- <https://www.gov.uk/company-tax-returns/making-changes>
- <https://www.gov.uk/annual-accounts/corrections-and-amendments>

## 3. Design decisions

1. Do not use Azure SQL Ledger or another proprietary database-integrity feature as part of seal validity.
2. Use documented canonical bytes, standard cryptographic hashes and standard asymmetric signatures.
3. Keep statutory filing evidence distinct from derived calculation checkpoint state.
4. Retain exact prepared and approved payload bytes; do not regenerate an allegedly identical artifact for dispatch or verification.
5. Represent creation, approval, dispatch, authority outcome, invalidation, correction and supersession as append-only events.
6. Preserve a complete genesis reconstruction path as the correctness oracle.
7. Fail closed when the source boundary, payload, approval, environment, tenant, software version or authority state is stale or inconsistent.
8. Store no ordinary financial values, tax identifiers, payload contents, business names or user contact details in operational telemetry.

## 4. Threat model

The design must detect or prevent:

- an ordinary user silently changing a protected historical fact;
- a privileged application path bypassing the correction workflow;
- dispatching bytes different from those reviewed and approved;
- replaying an approval against another tenant, period, authority, environment or software version;
- replacing a stored payload, receipt or manifest;
- deleting or reordering seal events;
- restoring a database to a state inconsistent with retained external evidence;
- an operator or database administrator rewriting both business data and database-local audit rows;
- concurrent writes crossing the source boundary while a seal is being prepared or activated; and
- retrying an uncertain submission as a new send rather than reconciling the original operation.

No single database-local hash defeats a database administrator who can rewrite both content and hashes. Signed manifests and independently protected root evidence therefore sit outside the tenant database.

## 5. Source boundary

Every accounting event that can alter a statutory result requires a durable, monotonically increasing `AccountingEventId`. A seal records an inclusive boundary identifier and an unambiguous business-period end. Dates, timestamps, SQL row versions and payload hashes alone are not sufficient boundaries.

Preparation must use one consistent database view. Before activation the system verifies that no event at or below the boundary was added, removed, replaced or reclassified after preparation. New events beyond the boundary may continue only where the accounting contract proves that they cannot affect the prepared period.

The protected source inventory must be explicit. It includes transaction facts, classifications, tax treatment, opening state and configuration that can alter the authority result. Presentation-only settings are excluded.

## 6. Canonical seal package

The retained package contains exact immutable byte objects plus a canonical manifest. The manifest uses a versioned, published encoding contract: UTF-8, deterministic property ordering, invariant dates and identifiers, and financial amounts represented without locale-dependent formatting or binary floating point.

The manifest records at least:

- protocol and manifest version;
- opaque seal, tenant, database-instance and reporting-subject identifiers;
- environment and authority family;
- obligation or reporting-period identity;
- inclusive accounting-event boundary and business-period end;
- hashes of the protected-source extraction and each prepared artifact;
- exact approved-payload hash and length;
- application, database-schema, calculation and authority-contract versions;
- preparation, review and approval event references;
- stable actor identifiers plus the displayed user names captured at the event;
- UTC technical timestamps and explicit business-zone identifiers;
- predecessor, correction and supersession references;
- statement opening and closing tax balances, with liability, adjustment, payment and repayment components identified separately from the authority payload;
- dispatch operation, idempotency and authority-correlation references;
- authority request, response, receipt and read-back hashes; and
- external evidence location, version and retention metadata.

The signature covers the exact canonical manifest bytes. The private signing key remains outside the database. Algorithm, key identity and rotation metadata are explicit so that old evidence remains verifiable after rotation.

## 7. Append-only lifecycle

The lifecycle is expressed as events rather than mutable status history:

1. `Prepared` — source boundary and exact artifacts captured.
2. `Reconciled` — genesis calculation and protected-source digest agree with the candidate.
3. `Reviewed` — an authorised human reviewed the identified artifacts.
4. `Approved` — a separate explicit action approved those exact hashes for one authority operation.
5. `Dispatched` — the exact approved bytes were sent once under the retained operation identity.
6. `Acknowledged` — the authority conversation and receipt were retained.
7. `ReadBackReconciled` — where supported, the authority state agrees with the retained submission.
8. `Invalidated` — the evidence cannot be used for a new send or calculation root.
9. `Superseded` — a correction or amended filing identifies the replacement seal while preserving this one.

An event cannot erase or revise an earlier event. A deterministic projection may expose the current state, but the event sequence remains authoritative.

## 8. VAT proving implementation

VAT is the first implementation because its obligation period, nine-box payload, submission correlation and read-back result provide a bounded end-to-end case.

For one VAT obligation the implementation must:

- capture the statement opening balance and the liability, payment, repayment and adjustment movements that produce its closing balance;
- capture the exact source boundary and calculated nine-box values;
- capture the exact VAT adjustment and owner explanation, requiring an explanation for every non-zero adjustment;
- retain the exact JSON bytes shown for approval;
- bind approval to tenant, VAT registration, obligation period, environment, payload hash and software version;
- reject a send if any binding value changed;
- use one durable idempotent operation across retries and restarts;
- retain fraud-prevention-header evidence without secrets or unnecessary personal data;
- retain request, response, receipt and read-back evidence;
- reconcile the accepted return to the prepared seal;
- detect a later historical change affecting the sealed period; and
- route corrections through a new linked seal rather than altering the accepted evidence.

HMRC acceptance proves receipt of a particular return. It does not itself prove the correctness of every source transaction or the tax-statement payment balance, so authority acknowledgement never replaces genesis reconciliation.

## 9. Tax balance checkpoint

The portable checkpoint contract mirrors each activated VAT or Corporation Tax statement boundary in the tenant database, provisionally through `Cash.tbTaxBalance`, and binds it to the corresponding signed seal package.

The checkpoint records opening balance, period liability, explained adjustments, payments or repayments and closing balance. The preceding closing balance must equal the next opening balance. Period close reconstructs the candidate from protected source data, reconciles its statutory component to the applicable submission seal, and compares it with an independent full-history calculation before activation.

The database row is not self-authenticating evidence and is never the only copy. A checkpoint with a stale source boundary, broken predecessor, mismatched seal, incompatible calculation version or failed genesis reconciliation is invalid and cannot become a calculation root.

## 10. Storage and portability

The logical contract has three stores:

- **Tenant accounting store:** source facts, boundary journal and portable seal projection.
- **Workflow store:** tenant-partitioned append-only authority and seal events.
- **Evidence store:** content-addressed immutable objects containing exact payloads, manifests, signatures and authority evidence.

The provider interfaces must support Azure SQL and Blob Storage for the hosted product without exposing those implementations in the evidence format. A PostgreSQL migration or an externally hosted installation must be able to import, verify and continue the same chain.

Versioning and soft deletion are not a substitute for locked immutable retention where a deployment's assurance contract requires it. The retention period and its deletion, privacy, correction and disaster-recovery consequences require explicit approval before any policy is locked.

## 11. Verification

A standalone verifier must be able to consume exported evidence and report:

- canonical-manifest and signature validity;
- predecessor-chain continuity;
- payload, artifact, response and receipt hash matches;
- tenant, database-instance, environment and period consistency;
- key identity and rotation-chain validity;
- authority-operation and supersession consistency; and
- whether a current genesis reconstruction still matches the protected source digest and sealed result.

The verifier must not need write access, ordinary application secrets or unrestricted tenant data. Verification failures create a high-priority audit event and disable affected submission paths until investigated.

## 12. Required tests

- Byte-identical preparation, approval and dispatch.
- One-bit payload, manifest, signature, artifact and receipt tampering.
- Missing, duplicated, reordered and cross-tenant events.
- Wrong period, authority, environment, database instance and software version.
- Concurrent posting during preparation and activation.
- Backdated insert, update, delete and reclassification attempts.
- Correction, amended filing, supersession and attempted reuse of superseded approval.
- Crash and restart before dispatch, during an uncertain outcome, and after acknowledgement.
- Backup restoration behind and ahead of externally retained evidence.
- Signing-key rotation and verification using retired public keys.
- Export from SQL Server and verification without SQL Server or Azure.
- Full genesis reconciliation and deliberate mismatch.
- Statement opening/closing continuity, including payments and repayments omitted from the authority payload.
- Broken or substituted tax-balance checkpoints and predecessor mismatches.

## 13. Delivery boundary

Stage 1 delivers the seal protocol, VAT proving implementation, tax-balance checkpoint, period-close validation, correction/supersession path and authority-neutral interfaces before VAT or Corporation Tax production dispatch is enabled. Corporation Tax and Companies House reuse the accepted evidence contract.

Production assurance proves restore, tamper detection, key rotation, external verification, operational response and cross-tenant isolation. A hosted deployment may use immutable object storage, but storage-provider controls do not define seal validity.

Stage 2 remains a later MIS-oriented optimisation. It may activate a validated checkpoint as the root of an individual calculation only after exact equivalence with genesis is proven and the improvement is material. Acceptance of this contract does not approve any such root change.

## 14. Open implementation decisions

The following require a focused implementation spike and review:

- canonical serialization profile and conformance vectors;
- signature algorithm, key provider, public-key retention and rotation;
- accounting-event journal coverage and migration of existing source paths;
- final `Cash.tbTaxBalance` schema, activation events and reporting-profile key;
- preparation concurrency and activation-lock semantics;
- evidence-store retention and immutability duration;
- whether approval requires one or two distinct human roles for the initial product scope; and
- the minimum standalone-verifier distribution and recovery format.
