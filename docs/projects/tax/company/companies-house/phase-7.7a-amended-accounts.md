# Phase 7.7A — amended accounts

## Purpose

Provide a controlled replacement-method revised-accounts journey when accepted Companies House
accounts are later found to be wrong. The cause may be a software defect, an accounting correction
or another non-software event. The correction is a Companies House filing lifecycle; Corporation Tax
is only a downstream consumer of the latest effective accepted accounts snapshot.

This phase was opened after downstream Corporation Tax reconciliation correctly found that accepted
accounts contained a zero Corporation Tax charge while the current filing-ready calculation produced
a non-zero charge. The accepted predecessor remains immutable evidence. It must not be edited,
deleted or made to agree by changing the tax calculation.

## Official mechanism

Companies House permits revised accounts to be filed through software that supports the operation
when the original accounts were filed through that software. The revised accounts cover the same
period and the original filing remains on the register.

For ordinary iXBRL accounts, Companies House TIS 6.2 uses the normal accounts-submission envelope.
The document itself identifies the revision. The supported replacement method requires:

- `ReportAnAmendedRevisedVersionPreviouslyFiledReportTruefalse = true`;
- a statement that the revised report replaces the previously filed report for the period;
- a statement that it is now the statutory accounts for the period;
- a statement that it was prepared as at the date of the previously filed report;
- the respects in which the previous report did not comply with the Companies Act 2006;
- the significant amendments made; and
- the date the revision was approved.

No inferred GovTalk envelope flag is introduced. Supplementary-note revisions, audited revisions and
revised ZIP packages are outside this first ordinary micro-company scope.

Official references:

- [Correcting and amending company accounts](https://www.gov.uk/annual-accounts/corrections-and-amendments)
- [Companies House technical interface specifications](https://www.gov.uk/government/publications/technical-interface-specifications-for-companies-house-software)

## Product lifecycle

1. Start only from a terminal accepted conversation belonging to the authenticated tenant, company,
   accounting period and supported filing profile.
2. Preserve the original preparation, approval, submitted bytes, authority response, snapshot and
   Category Tree evidence unchanged.
3. Open one explicit correction journey. Ordinary preparation continues to refuse a second filing
   for the same logical period.
4. Rebuild the accounts from current Trade Control transactions and collect attributable statements
   describing the non-compliance and significant amendments.
5. Produce filleted and full revised iXBRL documents containing the FRC 2026 replacement-method facts.
6. Compare the original and proposed accounts in business terms and retain the exact old/new document
   identities before approval.
7. Require a fresh director approval. The approval and successor filing identity link
   cryptographically to the accepted predecessor conversation.
8. Submit and track the successor through the existing exactly-once Companies House conversation.
9. Keep the predecessor unavailable to downstream CT preparation from the moment a correction draft
   exists. Discarding an unapproved correction releases that quarantine. Once the successor is
   accepted, its snapshot becomes the effective accounts evidence and the predecessor remains history.
10. If a CT600 has already been filed, require a separate HMRC amended-return workflow. Revised
    Companies House accounts never mutate or silently resubmit a CT return.

## Security and failure boundaries

- Preparing revised accounts requires Manager or Administrator permission.
- Pending, rejected, foreign-tenant, mismatched-period and recovery-required filings cannot be used as
  accepted predecessors.
- The browser supplies only the accepted conversation reference and reviewed correction narratives;
  company identity, period, source figures, retained evidence and credentials remain server-derived.
- A correction draft cannot weaken Category Tree sealing, stale-source checks, immutable approval,
  one-send dispatch or protected-content verification.
- The old accepted snapshot is fail-closed for CT while a correction is unresolved.
- Rejection never turns the old known-wrong accounts back into valid CT evidence. A further correction
  attempt requires an explicit retained-history decision rather than an ordinary resubmit button.

## Delivery increments

### Increment 1 — statutory contract and successor boundary

**Status: implemented and deployed; product preparation review passed.**

- Added the FRC 2026 revised-report marker and six replacement-method facts to the statutory accounts
  contract, projection, validator and human-readable iXBRL.
- Added explicit amended preparation, predecessor validation, successor logical identity and retained
  lineage metadata.
- Added Manager/Administrator authorisation and preserved the ordinary duplicate-filing refusal.
- Added an Accounts workspace correction entry point and reviewed correction narratives.
- Quarantined an accepted predecessor from Corporation Tax as soon as an uninvalidated amendment
  preparation exists.
- Made the authenticated principal explicit at every interactive Companies House workflow boundary.
  This preserves fail-closed tenant and role checks when a server-side UI event is no longer running
  inside an ordinary HTTP request.

### Increment 2 — comparison and development acceptance exercise

**Status: controlled official-test amendment exercise passed; Corporation Tax reconciliation passed.**

- present an exact business-level old/new comparison, including tax on profit, profit after tax,
  capital and reserves, retained document SHA-256 values and predecessor identity — implemented;
- exercise prepare, preview, discard and fresh approval in the controlled development environment;
- stage only through the official-test composition and simulate acceptance only through the existing
  visibly labelled development control;
- prove that the revised accepted snapshot replaces its accepted predecessor for CT continuation
  while both filings remain available as Companies House history; and
- rerun the CT reconciliation and require accounting profit and tax charge to agree before CT filing
  work recommences.

The amended-accounts journey is deployed in the development environment and has passed its controlled
acceptance exercise. The controlled preparation review proved a non-zero Corporation Tax correction,
an equal reduction in profit after tax and no change to capital and reserves. That distinction was
used to invalidate an inaccurate unapproved draft and prepare a replacement whose statutory revision
statements match the retained old/new comparison. Review then exposed a presentation defect: the
machine-readable revised-report Boolean was visible before the human-readable statement. The renderer
now places that fact in the Inline XBRL hidden header while retaining clean visible prose. The affected
unapproved artifact was invalidated and a fresh artifact passed both checks. The predecessor remains
immutable. The corrected successor was approved, acknowledged by the Companies House official-test
gateway and then accepted only through the visibly labelled development simulation. The resulting
accepted full-accounts snapshot reconciles to the current transaction-derived accounting profit and
Corporation Tax charge with zero difference.

## Acceptance gate

An authorised development user can start from an accepted predecessor, review the old/new statutory
difference, prepare and approve replacement-method revised accounts, observe a distinct successor
conversation, and obtain a new explicitly simulated accepted snapshot. The original evidence remains
immutable. Corporation Tax cannot consume the predecessor while the correction is open and can
consume only the accepted successor once its accounting profit and Corporation Tax charge reconcile
to current transactions.

Production Companies House dispatch remains disabled. The official-test amendment exercise does not
constitute a live filing, and its acceptance was simulated rather than returned by Companies House.
The amended-accounts CT reconciliation gate is satisfied; later Corporation Tax transport and
submission gates remain subject to the Corporation Tax roadmap.
